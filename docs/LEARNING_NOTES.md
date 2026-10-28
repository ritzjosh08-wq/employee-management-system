# Learning notes: how this app works

A refresher on C#, ASP.NET Core MVC and SQL Server, using this project as the example.
Read it with the code open next to it.

## 1. What happens when you open /Employees?search=ana

1. **Routing.** `Program.cs` maps the pattern `{controller=Home}/{action=Index}/{id?}`.
   `/Employees` means controller `EmployeesController`, action `Index`.
2. **Model binding.** ASP.NET Core reads `search=ana` from the query string and passes it into
   `Index(string? search, int? departmentId, int page = 1)`. Names must match.
3. **Dependency injection.** The controller's constructor asks for `IEmployeeRepository`.
   `Program.cs` registered `AddScoped<IEmployeeRepository, EmployeeRepository>()`, so the framework
   creates one `EmployeeRepository` per request and hands it in. The controller only knows the interface.
4. **Data access.** `EmployeeRepository.SearchAsync` opens a `SqlConnection`, builds a `SqlCommand`
   with parameters, runs it and reads rows with a `SqlDataReader`, turning each row into an `Employee`.
5. **View model.** The controller packs the results, filter values and drop-down options into
   `EmployeeListViewModel` and calls `return View(model)`.
6. **Razor view.** `Views/Employees/Index.cshtml` (found by convention: `Views/{Controller}/{Action}`)
   renders HTML. `@model` declares the type; `@foreach`, `@Model.X` and tag helpers like
   `asp-action="Details"` produce the markup. `_Layout.cshtml` wraps every page.

## 2. MVC in one line each

- **Model**: C# classes holding data (`Employee`, `Department`). Data annotations like
  `[Required]`, `[StringLength(50)]` drive validation.
- **View**: `.cshtml` files; HTML plus C# after `@`.
- **Controller**: classes ending in `Controller`; public methods are actions that return `IActionResult`
  (`View()`, `RedirectToAction()`, `NotFound()`, `BadRequest()`).

## 3. Forms: the GET/POST pattern

- `GET /Employees/Create` shows an empty form.
- `POST /Employees/Create` receives it. The action checks `ModelState.IsValid`:
  - invalid → return the same view, so the errors appear next to the fields;
  - valid → save, set `TempData["Success"]`, and **redirect** (Post/Redirect/Get, so refreshing
    the page doesn't submit twice).
- `asp-for="Employee.Email"` generates `name`, `id`, `value` and validation attributes.
- `[ValidateAntiForgeryToken]` + the hidden token the `<form>` tag helper adds = CSRF protection.
- `[Bind(Prefix = "Employee")]` tells binding that fields are named `Employee.FirstName` etc.

## 4. ADO.NET (the database connectivity layer)

```csharp
await using var connection = await _connectionFactory.CreateOpenConnectionAsync();  // open (from the pool)
await using var command = new SqlCommand(sql, connection);                          // the SQL
command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = id;                    // safe input
await using var reader = await command.ExecuteReaderAsync();                        // run it
while (await reader.ReadAsync()) { /* reader.GetString(...) */ }                    // read rows
```

| Method | Use for | Returns |
|---|---|---|
| `ExecuteReaderAsync` | `SELECT` with rows | `SqlDataReader` |
| `ExecuteScalarAsync` | one value (`OUTPUT INSERTED.Id`, `COUNT(*)`) | `object` |
| `ExecuteNonQueryAsync` | `INSERT`/`UPDATE`/`DELETE` | rows affected |

- `await using` disposes the connection, which returns it to the **connection pool**. Open late, close early.
- **Never** build SQL by concatenating user input. Always use parameters (SQL injection).
- `DBNull.Value` is how you send/receive SQL `NULL`.
- Stored procedure: set `CommandType = CommandType.StoredProcedure` and use the procedure name as the text
  (see `DepartmentRepository.GetSummaryAsync`).
- SQL errors arrive as `SqlException`; `ex.Number` tells you which (2627/2601 = duplicate key,
  547 = foreign key/check constraint). See `Data/SqlErrors.cs`.

## 5. SQL Server concepts used

- **Primary key** (`PK_Employees`), **foreign key** (`FK_Employees_Departments`),
  **unique** (`UQ_Employees_Email`) and **check** (`CK_Employees_Salary`) constraints.
- **IDENTITY(1,1)** auto-increments IDs; `OUTPUT INSERTED.EmployeeId` returns the new one.
- **JOIN**: `INNER JOIN` (employees always have a department) vs `LEFT JOIN`
  (departments with zero employees still appear).
- **GROUP BY + COUNT/AVG** for summaries.
- **Paging**: `ORDER BY ... OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY`.
- **Window function**: `COUNT(*) OVER()` returns the total row count on every row.
- **Indexes**: non-clustered indexes on columns used in `WHERE`, `JOIN` and `ORDER BY`.
- **GO** separates batches in SSMS/sqlcmd scripts; it isn't T-SQL, which is why `DatabaseInitializer` splits on it.

## 6. Configuration

- `appsettings.json` → `appsettings.{Environment}.json` → user secrets (Development) → environment variables.
  Later sources override earlier ones.
- `builder.Configuration.GetConnectionString("DefaultConnection")` reads `ConnectionStrings:DefaultConnection`.
- Env var form uses double underscores: `ConnectionStrings__DefaultConnection`.

## 7. Useful commands

```bash
dotnet new mvc -n MyApp           # new MVC project
dotnet build                      # compile
dotnet run                        # run (uses Properties/launchSettings.json)
dotnet watch                      # run + reload on save
dotnet add package <Name>         # add a NuGet package
dotnet user-secrets set "Key" "Value"
```

## 8. Interview-style questions to practise

1. What's the difference between `AddSingleton`, `AddScoped` and `AddTransient`? Why are repositories scoped?
2. Why redirect after a successful POST?
3. How does this app prevent SQL injection? Over-posting? CSRF?
4. Why is there no `ON DELETE CASCADE` on the employee → department foreign key?
5. `ExecuteScalar` vs `ExecuteNonQuery` vs `ExecuteReader`?
6. What would change if you switched this data layer to Entity Framework Core?
