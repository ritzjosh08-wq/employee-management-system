using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repositories;
using EmployeeManagement.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Controllers;

public class EmployeesController : Controller
{
    private const int PageSize = 10;

    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;

    public EmployeesController(IEmployeeRepository employees, IDepartmentRepository departments)
    {
        _employees = employees;
        _departments = departments;
    }

    // GET: /Employees?search=ana&departmentId=2&isActive=true&sort=salary_desc&page=1
    public async Task<IActionResult> Index([FromQuery] EmployeeQuery query, CancellationToken cancellationToken)
    {
        // The page size is fixed by the server, not taken from the query string.
        query = query with { Page = Math.Max(query.Page, 1), PageSize = PageSize };

        var model = new EmployeeListViewModel
        {
            Result = await _employees.SearchAsync(query, cancellationToken),
            Query = query,
            Departments = await GetDepartmentOptionsAsync(query.DepartmentId, cancellationToken)
        };

        return View(model);
    }

    // GET: /Employees/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    // GET: /Employees/Create
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await BuildFormAsync(new Employee(), cancellationToken));
    }

    // POST: /Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Employee")] Employee employee,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(employee, cancellationToken));
        }

        try
        {
            var id = await _employees.CreateAsync(employee, cancellationToken);
            TempData["Success"] = $"{employee.FullName} was added.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (SqlException ex) when (SqlErrors.IsUniqueViolation(ex))
        {
            ModelState.AddModelError("Employee.Email", "Another employee already uses this email address.");
            return View(await BuildFormAsync(employee, cancellationToken));
        }
    }

    // GET: /Employees/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : View(await BuildFormAsync(employee, cancellationToken));
    }

    // POST: /Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Employee")] Employee employee,
        CancellationToken cancellationToken)
    {
        if (id != employee.EmployeeId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(employee, cancellationToken));
        }

        try
        {
            if (!await _employees.UpdateAsync(employee, cancellationToken))
            {
                return NotFound();
            }
        }
        catch (SqlException ex) when (SqlErrors.IsUniqueViolation(ex))
        {
            ModelState.AddModelError("Employee.Email", "Another employee already uses this email address.");
            return View(await BuildFormAsync(employee, cancellationToken));
        }

        TempData["Success"] = $"{employee.FullName} was updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Employees/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    // POST: /Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _employees.DeleteAsync(id, cancellationToken);
        TempData["Success"] = "Employee was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<EmployeeFormViewModel> BuildFormAsync(Employee employee, CancellationToken cancellationToken)
    {
        return new EmployeeFormViewModel
        {
            Employee = employee,
            Departments = await GetDepartmentOptionsAsync(employee.DepartmentId, cancellationToken)
        };
    }

    private async Task<IEnumerable<SelectListItem>> GetDepartmentOptionsAsync(int? selectedId,
        CancellationToken cancellationToken)
    {
        var departments = await _departments.GetAllAsync(cancellationToken);
        return departments.Select(d => new SelectListItem(d.Name, d.DepartmentId.ToString(), d.DepartmentId == selectedId));
    }
}
