/*
    Sample data. Only inserted when the tables are empty, so it never overwrites real data.
    All people below are fictional.
*/

IF NOT EXISTS (SELECT 1 FROM dbo.Departments)
BEGIN
    INSERT INTO dbo.Departments (Name, Location)
    VALUES (N'Engineering',      N'Building A'),
           (N'Finance',          N'Building B'),
           (N'Human Resources',  N'Building B'),
           (N'Operations',       N'Warehouse'),
           (N'Sales',            N'Building C');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Employees)
BEGIN
    DECLARE @Engineering INT = (SELECT DepartmentId FROM dbo.Departments WHERE Name = N'Engineering');
    DECLARE @Finance     INT = (SELECT DepartmentId FROM dbo.Departments WHERE Name = N'Finance');
    DECLARE @HR          INT = (SELECT DepartmentId FROM dbo.Departments WHERE Name = N'Human Resources');
    DECLARE @Operations  INT = (SELECT DepartmentId FROM dbo.Departments WHERE Name = N'Operations');
    DECLARE @Sales       INT = (SELECT DepartmentId FROM dbo.Departments WHERE Name = N'Sales');

    INSERT INTO dbo.Employees (FirstName, LastName, Email, JobTitle, Salary, HireDate, IsActive, DepartmentId)
    VALUES (N'Ava',     N'Thompson', N'ava.thompson@example.com',     N'Software Engineer',        92000, '2021-03-15', 1, @Engineering),
           (N'Liam',    N'Patel',    N'liam.patel@example.com',       N'Senior Software Engineer', 118000, '2019-07-01', 1, @Engineering),
           (N'Mia',     N'Chen',     N'mia.chen@example.com',         N'QA Engineer',              78000, '2022-01-10', 1, @Engineering),
           (N'Noah',    N'Garcia',   N'noah.garcia@example.com',      N'DevOps Engineer',          99000, '2023-05-22', 1, @Engineering),
           (N'Zara',    N'Okafor',   N'zara.okafor@example.com',      N'Engineering Manager',      135000, '2018-09-03', 1, @Engineering),
           (N'Ethan',   N'Brooks',   N'ethan.brooks@example.com',     N'Financial Analyst',        74000, '2022-08-29', 1, @Finance),
           (N'Sofia',   N'Rossi',    N'sofia.rossi@example.com',      N'Accountant',               69000, '2020-02-17', 1, @Finance),
           (N'Lucas',   N'Nguyen',   N'lucas.nguyen@example.com',     N'Finance Director',         142000, '2017-11-06', 1, @Finance),
           (N'Isla',    N'Murphy',   N'isla.murphy@example.com',      N'HR Generalist',            61000, '2023-02-13', 1, @HR),
           (N'Omar',    N'Haddad',   N'omar.haddad@example.com',      N'Recruiter',                58000, '2024-04-08', 1, @HR),
           (N'Grace',   N'Kim',      N'grace.kim@example.com',        N'Operations Coordinator',   55000, '2021-10-25', 1, @Operations),
           (N'Mateo',   N'Silva',    N'mateo.silva@example.com',      N'Logistics Lead',           67000, '2019-04-14', 0, @Operations),
           (N'Chloe',   N'Dubois',   N'chloe.dubois@example.com',     N'Account Executive',        72000, '2022-06-06', 1, @Sales),
           (N'Arjun',   N'Mehta',    N'arjun.mehta@example.com',      N'Sales Manager',            98000, '2018-01-29', 1, @Sales),
           (N'Hannah',  N'Weber',    N'hannah.weber@example.com',     N'Sales Representative',     52000, '2024-09-16', 1, @Sales);
END
GO
