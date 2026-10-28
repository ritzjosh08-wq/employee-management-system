using System.Data;
using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Repositories;

// ADO.NET repository: every query is parameterized SQL (or a stored procedure),
// which protects against SQL injection and lets SQL Server reuse query plans.
public class DepartmentRepository : IDepartmentRepository
{
    private const string SelectDepartments = """
        SELECT d.DepartmentId, d.Name, d.Location, d.CreatedAt,
               COUNT(e.EmployeeId) AS EmployeeCount
        FROM dbo.Departments AS d
        LEFT JOIN dbo.Employees AS e ON e.DepartmentId = d.DepartmentId
        """;

    private const string GroupByDepartment = """
        GROUP BY d.DepartmentId, d.Name, d.Location, d.CreatedAt
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public DepartmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"{SelectDepartments}\n{GroupByDepartment}\nORDER BY d.Name;";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var departments = new List<Department>();
        while (await reader.ReadAsync(cancellationToken))
        {
            departments.Add(Map(reader));
        }
        return departments;
    }

    public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = $"{SelectDepartments}\nWHERE d.DepartmentId = @DepartmentId\n{GroupByDepartment};";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<int> CreateAsync(Department department, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Departments (Name, Location)
            OUTPUT INSERTED.DepartmentId
            VALUES (@Name, @Location);
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, department);

        return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    public async Task<bool> UpdateAsync(Department department, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Departments
            SET Name = @Name, Location = @Location
            WHERE DepartmentId = @DepartmentId;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, department);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = department.DepartmentId;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM dbo.Departments WHERE DepartmentId = @DepartmentId;";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = id;

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<IReadOnlyList<DepartmentSummary>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_GetDepartmentSummary", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var summary = new List<DepartmentSummary>();
        while (await reader.ReadAsync(cancellationToken))
        {
            summary.Add(new DepartmentSummary(
                reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                reader.GetString(reader.GetOrdinal("Name")),
                reader.GetInt32(reader.GetOrdinal("EmployeeCount")),
                reader.GetDecimal(reader.GetOrdinal("AverageSalary"))));
        }
        return summary;
    }

    private static void AddParameters(SqlCommand command, Department department)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = department.Name.Trim();
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 100).Value =
            string.IsNullOrWhiteSpace(department.Location) ? DBNull.Value : department.Location.Trim();
    }

    private static Department Map(SqlDataReader reader)
    {
        var locationOrdinal = reader.GetOrdinal("Location");

        return new Department
        {
            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Location = reader.IsDBNull(locationOrdinal) ? null : reader.GetString(locationOrdinal),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount"))
        };
    }
}
