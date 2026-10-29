using System.Data;
using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private const string SelectEmployees = """
        SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.JobTitle, e.Salary,
               e.HireDate, e.IsActive, e.DepartmentId, d.Name AS DepartmentName
        FROM dbo.Employees AS e
        INNER JOIN dbo.Departments AS d ON d.DepartmentId = e.DepartmentId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public EmployeeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Column names can't be SQL parameters, so the sort order comes from this fixed
    // allow-list. Anything not in it falls back to sorting by name.
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        [EmployeeSort.Name] = "e.LastName {0}, e.FirstName {0}",
        [EmployeeSort.Department] = "d.Name {0}, e.LastName, e.FirstName",
        [EmployeeSort.JobTitle] = "e.JobTitle {0}, e.LastName, e.FirstName",
        [EmployeeSort.Salary] = "e.Salary {0}, e.LastName, e.FirstName",
        [EmployeeSort.HireDate] = "e.HireDate {0}, e.LastName, e.FirstName"
    };

    public async Task<PagedResult<Employee>> SearchAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        // COUNT(*) OVER() returns the total number of matching rows on every row,
        // so one round trip gives us both the page and the total for the pager.
        // OFFSET/FETCH does the paging inside SQL Server.
        var sql = $"""
            SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.JobTitle, e.Salary,
                   e.HireDate, e.IsActive, e.DepartmentId, d.Name AS DepartmentName,
                   COUNT(*) OVER() AS TotalCount
            FROM dbo.Employees AS e
            INNER JOIN dbo.Departments AS d ON d.DepartmentId = e.DepartmentId
            WHERE (@Search IS NULL
                   OR e.FirstName LIKE @Search OR e.LastName LIKE @Search
                   OR e.Email LIKE @Search OR e.JobTitle LIKE @Search)
              AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
              AND (@IsActive IS NULL OR e.IsActive = @IsActive)
            ORDER BY {BuildOrderBy(query.Sort)}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 260).Value =
            string.IsNullOrWhiteSpace(query.Search) ? DBNull.Value : $"%{EscapeLike(query.Search.Trim())}%";
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = (object?)query.DepartmentId ?? DBNull.Value;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = (object?)query.IsActive ?? DBNull.Value;
        command.Parameters.Add("@Offset", SqlDbType.Int).Value = (query.Page - 1) * query.PageSize;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = query.PageSize;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var employees = new List<Employee>();
        var totalCount = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            employees.Add(Map(reader));
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
        }

        return new PagedResult<Employee>
        {
            Items = employees,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    private static string BuildOrderBy(string? sort)
    {
        var descending = sort?.EndsWith(EmployeeSort.DescendingSuffix, StringComparison.OrdinalIgnoreCase) == true;
        var key = descending ? sort![..^EmployeeSort.DescendingSuffix.Length] : sort ?? EmployeeSort.Name;

        var template = SortColumns.GetValueOrDefault(key, SortColumns[EmployeeSort.Name]);
        return string.Format(template, descending ? "DESC" : "ASC");
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = $"{SelectEmployees}\nWHERE e.EmployeeId = @EmployeeId;";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<int> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Employees
                (FirstName, LastName, Email, JobTitle, Salary, HireDate, IsActive, DepartmentId)
            OUTPUT INSERTED.EmployeeId
            VALUES
                (@FirstName, @LastName, @Email, @JobTitle, @Salary, @HireDate, @IsActive, @DepartmentId);
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, employee);

        return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    public async Task<bool> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Employees
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                JobTitle = @JobTitle,
                Salary = @Salary,
                HireDate = @HireDate,
                IsActive = @IsActive,
                DepartmentId = @DepartmentId,
                UpdatedAt = SYSUTCDATETIME()
            WHERE EmployeeId = @EmployeeId;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, employee);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employee.EmployeeId;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM dbo.Employees WHERE EmployeeId = @EmployeeId;";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = id;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<Employee>> GetRecentHiresAsync(int count, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            {SelectEmployees}
            WHERE e.IsActive = 1
            ORDER BY e.HireDate DESC
            OFFSET 0 ROWS FETCH NEXT @Count ROWS ONLY;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Count", SqlDbType.Int).Value = count;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var employees = new List<Employee>();
        while (await reader.ReadAsync(cancellationToken))
        {
            employees.Add(Map(reader));
        }
        return employees;
    }

    public async Task<EmployeeStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) AS TotalEmployees,
                   ISNULL(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END), 0) AS ActiveEmployees,
                   CAST(ISNULL(AVG(Salary), 0) AS DECIMAL(12, 2)) AS AverageSalary
            FROM dbo.Employees;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);
        return new EmployeeStats(
            reader.GetInt32(reader.GetOrdinal("TotalEmployees")),
            reader.GetInt32(reader.GetOrdinal("ActiveEmployees")),
            reader.GetDecimal(reader.GetOrdinal("AverageSalary")));
    }

    private static void AddParameters(SqlCommand command, Employee employee)
    {
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = employee.FirstName.Trim();
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = employee.LastName.Trim();
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = employee.Email.Trim();
        command.Parameters.Add("@JobTitle", SqlDbType.NVarChar, 100).Value = employee.JobTitle.Trim();

        var salary = command.Parameters.Add("@Salary", SqlDbType.Decimal);
        salary.Precision = 12;
        salary.Scale = 2;
        salary.Value = employee.Salary;

        command.Parameters.Add("@HireDate", SqlDbType.Date).Value = employee.HireDate.Date;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = employee.IsActive;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = employee.DepartmentId;
    }

    // Treat %, _ and [ typed by the user as literal characters, not LIKE wildcards.
    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    private static Employee Map(SqlDataReader reader) => new()
    {
        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        JobTitle = reader.GetString(reader.GetOrdinal("JobTitle")),
        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
        DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName"))
    };
}
