# Employee Management System

A full-stack web application built with **C#**, **ASP.NET Core MVC** and **SQL Server**.
It manages employees and departments: CRUD screens, search, filtering, paging and a dashboard.

> This is a personal portfolio project. It demonstrates the stack and patterns I worked with during my
> full-stack development internship (enterprise MVC architecture, a hand-written SQL Server data access
> layer, relational database design). It is not the employer's code.

![build](https://github.com/ritzjosh08-wq/employee-management-system/actions/workflows/build.yml/badge.svg)

## Features

- **Dashboard**: total and active headcount, average salary, per-department summary (from a stored procedure), recent hires
- **Employees**: list with search (name, email, job title), filter by department and server-side paging; create, view, edit and delete
- **Departments**: CRUD, with employee counts; a department that still has employees can't be deleted
- **Validation** on both sides: data annotations + jQuery unobtrusive validation in the browser, and constraints in SQL Server
- **Friendly errors** for duplicate emails and department names (unique constraints mapped to form errors)
- **Database bootstrapping**: the app creates the database, tables, indexes, stored procedure and sample data on first run

## Tech stack

| Layer | Technology |
|---|---|
| Language | C# 14 / .NET 10 |
| Web | ASP.NET Core MVC, Razor views, tag helpers |
| Data access | ADO.NET with `Microsoft.Data.SqlClient` (parameterized SQL + stored procedure) |
| Database | SQL Server (LocalDB, SQL Server Express/Developer, or Docker) |
| UI | Bootstrap 5, jQuery Validation |
| CI | GitHub Actions (`dotnet build`) |

## Architecture

```
Browser ──HTTP──▶ Controller ──▶ Repository (interface) ──▶ SqlConnection / SqlCommand ──▶ SQL Server
                     │                                                                     
                     ▼                                                                     
               View model ──▶ Razor view ──HTML──▶ Browser
```

- **Controllers** (`Controllers/`) handle requests, validate input and pick a view. They never contain SQL.
- **Repositories** (`Repositories/`) are the data access layer. Each one sits behind an interface
  (`IEmployeeRepository`, `IDepartmentRepository`) and is registered with dependency injection in `Program.cs`.
- **`IDbConnectionFactory`** (`Data/`) creates open, pooled SQL connections from the configured connection string.
- **Models** (`Models/`) mirror the database tables; **view models** (`ViewModels/`) shape data for a specific page.
- **Database scripts** (`database/`) hold the schema and seed data, and are run automatically on startup.

### Database design

```
Departments                         Employees
───────────                         ─────────
DepartmentId  PK  ◀──────┐          EmployeeId    PK
Name          UNIQUE     └──────── DepartmentId  FK (no cascade delete)
Location                            FirstName, LastName
CreatedAt                           Email         UNIQUE
                                    JobTitle
                                    Salary        CHECK (>= 0)
                                    HireDate, IsActive
                                    CreatedAt, UpdatedAt
```

Indexes on `Employees(DepartmentId)` and `Employees(LastName, FirstName)` back the filter and the default sort.
`dbo.usp_GetDepartmentSummary` returns active headcount and average salary per department.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A SQL Server instance (pick one option below)

### Option A: Windows with SQL Server LocalDB

LocalDB is installed with Visual Studio. The default connection string in `appsettings.json` already points to it:

```
Server=(localdb)\MSSQLLocalDB;Database=EmployeeManagementDb;Trusted_Connection=True;TrustServerCertificate=True
```

```bash
dotnet run --project src/EmployeeManagement.Web
```

### Option B: macOS / Linux with Docker

```bash
export MSSQL_SA_PASSWORD='Choose-A-Str0ng-Password'
docker compose up -d
```

Point the app at the container with .NET user secrets (keeps the password out of the repo):

```bash
cd src/EmployeeManagement.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=EmployeeManagementDb;User Id=sa;Password=$MSSQL_SA_PASSWORD;TrustServerCertificate=True"
dotnet run
```

Open http://localhost:5080. On first start the app creates `EmployeeManagementDb` and loads sample data.

To manage the schema yourself instead, set `"Database": { "InitializeOnStartup": false }` and run
`database/schema.sql` then `database/seed.sql` in SSMS, Azure Data Studio or `sqlcmd`.

## Project structure

```
├── database/
│   ├── schema.sql              # tables, constraints, indexes, stored procedure
│   └── seed.sql                # sample departments and employees
├── src/EmployeeManagement.Web/
│   ├── Controllers/            # HomeController, EmployeesController, DepartmentsController
│   ├── Data/                   # connection factory, database initializer, SQL error helpers
│   ├── Models/                 # Employee, Department, PagedResult, ...
│   ├── Repositories/           # ADO.NET data access layer
│   ├── ViewModels/             # page-specific models
│   ├── Views/                  # Razor views
│   ├── wwwroot/                # static files
│   └── Program.cs              # dependency injection + middleware pipeline
├── docs/LEARNING_NOTES.md      # how the pieces fit together
└── docker-compose.yml          # SQL Server for macOS/Linux
```

## Security notes

- Every query uses `SqlParameter`s with explicit types, so user input is never concatenated into SQL.
  User-entered `%`, `_` and `[` are escaped before `LIKE` searches.
- All POST actions use `[ValidateAntiForgeryToken]`; model binding uses `[Bind]` to prevent over-posting.
- No credentials are committed. Use user secrets or environment variables (`ConnectionStrings__DefaultConnection`).

## Ideas for next steps

- Authentication and roles with ASP.NET Core Identity
- Unit tests for controllers (mocking the repository interfaces) and integration tests against a test database
- Audit log table for employee changes
- Export the employee list to CSV
