using EmployeeManagement.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.Web.ViewModels;

public class EmployeeListViewModel
{
    public required PagedResult<Employee> Result { get; init; }
    public required IEnumerable<SelectListItem> Departments { get; init; }
    public string? Search { get; init; }
    public int? DepartmentId { get; init; }
}
