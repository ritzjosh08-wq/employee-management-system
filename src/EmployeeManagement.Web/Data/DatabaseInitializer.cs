using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Data;

// Creates the database on first run and applies database/schema.sql and database/seed.sql.
// Both scripts are idempotent, so running this on every startup is safe.
public partial class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        var scriptsFolder = Path.Combine(AppContext.BaseDirectory, "Database");
        await RunScriptAsync(Path.Combine(scriptsFolder, "schema.sql"), cancellationToken);
        await RunScriptAsync(Path.Combine(scriptsFolder, "seed.sql"), cancellationToken);

        _logger.LogInformation("Database is ready.");
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(_connectionFactory.ConnectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // A database name can't be a SQL parameter, so build the statement inside SQL
        // with QUOTENAME to keep it safe from injection.
        const string sql = """
            IF DB_ID(@DatabaseName) IS NULL
            BEGIN
                DECLARE @sql NVARCHAR(400) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
                EXEC (@sql);
            END
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DatabaseName", databaseName);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task RunScriptAsync(string path, CancellationToken cancellationToken)
    {
        var script = await File.ReadAllTextAsync(path, cancellationToken);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        // "GO" is a batch separator understood by SSMS/sqlcmd, not by SQL Server itself,
        // so split the script and send each batch separately.
        foreach (var batch in GoSeparator().Split(script).Where(b => !string.IsNullOrWhiteSpace(b)))
        {
            await using var command = new SqlCommand(batch, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation("Applied {Script}", Path.GetFileName(path));
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}
