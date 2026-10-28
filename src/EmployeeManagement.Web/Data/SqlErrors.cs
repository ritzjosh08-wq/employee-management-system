using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Data;

public static class SqlErrors
{
    // 2627 = UNIQUE/PRIMARY KEY constraint violation, 2601 = duplicate key in a unique index.
    public static bool IsUniqueViolation(SqlException ex) => ex.Number is 2627 or 2601;

    // 547 = FOREIGN KEY or CHECK constraint conflict.
    public static bool IsConstraintViolation(SqlException ex) => ex.Number == 547;
}
