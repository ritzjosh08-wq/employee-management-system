using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.ViewModels;

public class DashboardViewModel
{
    public required EmployeeStats Stats { get; init; }
    public required IReadOnlyList<DepartmentSummary> Departments { get; init; }
    public required IReadOnlyList<Employee> RecentHires { get; init; }
}
