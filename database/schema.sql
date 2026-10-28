/*
    Employee Management System - schema
    Safe to run more than once: every object is only created if it is missing.
*/

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments
    (
        DepartmentId INT IDENTITY(1, 1) NOT NULL,
        Name         NVARCHAR(100)      NOT NULL,
        Location     NVARCHAR(100)      NULL,
        CreatedAt    DATETIME2(0)       NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (DepartmentId),
        CONSTRAINT UQ_Departments_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        EmployeeId   INT IDENTITY(1, 1) NOT NULL,
        FirstName    NVARCHAR(50)       NOT NULL,
        LastName     NVARCHAR(50)       NOT NULL,
        Email        NVARCHAR(256)      NOT NULL,
        JobTitle     NVARCHAR(100)      NOT NULL,
        Salary       DECIMAL(12, 2)     NOT NULL,
        HireDate     DATE               NOT NULL,
        IsActive     BIT                NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        DepartmentId INT                NOT NULL,
        CreatedAt    DATETIME2(0)       NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt    DATETIME2(0)       NULL,

        CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (EmployeeId),
        CONSTRAINT UQ_Employees_Email UNIQUE (Email),
        CONSTRAINT CK_Employees_Salary CHECK (Salary >= 0),
        -- No ON DELETE CASCADE: a department with employees cannot be deleted by accident.
        CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId)
            REFERENCES dbo.Departments (DepartmentId)
    );

    -- Speeds up the JOIN to Departments and the "filter by department" query.
    CREATE NONCLUSTERED INDEX IX_Employees_DepartmentId ON dbo.Employees (DepartmentId);

    -- Supports the default sort order of the employee list.
    CREATE NONCLUSTERED INDEX IX_Employees_LastName_FirstName ON dbo.Employees (LastName, FirstName);
END
GO

-- Active headcount and average salary per department (used on the dashboard).
CREATE OR ALTER PROCEDURE dbo.usp_GetDepartmentSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT d.DepartmentId,
           d.Name,
           COUNT(e.EmployeeId)                              AS EmployeeCount,
           CAST(ISNULL(AVG(e.Salary), 0) AS DECIMAL(12, 2)) AS AverageSalary
    FROM dbo.Departments AS d
    LEFT JOIN dbo.Employees AS e
        ON e.DepartmentId = d.DepartmentId
       AND e.IsActive = 1
    GROUP BY d.DepartmentId, d.Name
    ORDER BY d.Name;
END
GO
