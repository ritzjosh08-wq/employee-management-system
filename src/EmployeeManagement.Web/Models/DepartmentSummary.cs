namespace EmployeeManagement.Web.Models;

// One row returned by the dbo.usp_GetDepartmentSummary stored procedure.
public record DepartmentSummary(int DepartmentId, string Name, int EmployeeCount, decimal AverageSalary);
