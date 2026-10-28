using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Data;

public interface IDbConnectionFactory
{
    string ConnectionString { get; }

    // Returns an already-opened connection. The caller owns it and must dispose it
    // (use "await using"), which returns it to the ADO.NET connection pool.
    Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
