using EmployeeManagement.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.Web.ViewModels;

// Used by the Create and Edit pages: the employee being edited plus the
// department drop-down options.
public class EmployeeFormViewModel
{
    public Employee Employee { get; set; } = new();
    public IEnumerable<SelectListItem> Departments { get; set; } = [];
}
