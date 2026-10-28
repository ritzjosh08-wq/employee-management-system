using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.Web.Controllers;

public class DepartmentsController : Controller
{
    private readonly IDepartmentRepository _departments;

    public DepartmentsController(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    // GET: /Departments
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await _departments.GetAllAsync(cancellationToken));
    }

    // GET: /Departments/Create
    public IActionResult Create()
    {
        return View(new Department());
    }

    // POST: /Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Location")] Department department,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            await _departments.CreateAsync(department, cancellationToken);
        }
        catch (SqlException ex) when (SqlErrors.IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(Department.Name), "A department with this name already exists.");
            return View(department);
        }

        TempData["Success"] = $"Department \"{department.Name}\" was created.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Departments/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var department = await _departments.GetByIdAsync(id, cancellationToken);
        return department is null ? NotFound() : View(department);
    }

    // POST: /Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,Name,Location")] Department department,
        CancellationToken cancellationToken)
    {
        if (id != department.DepartmentId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            if (!await _departments.UpdateAsync(department, cancellationToken))
            {
                return NotFound();
            }
        }
        catch (SqlException ex) when (SqlErrors.IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(Department.Name), "A department with this name already exists.");
            return View(department);
        }

        TempData["Success"] = $"Department \"{department.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Departments/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var department = await _departments.GetByIdAsync(id, cancellationToken);
        return department is null ? NotFound() : View(department);
    }

    // POST: /Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _departments.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Department was deleted.";
        }
        catch (SqlException ex) when (SqlErrors.IsConstraintViolation(ex))
        {
            // The foreign key from Employees blocks deleting a department that still has staff.
            TempData["Error"] = "This department still has employees. Move or remove them first.";
        }

        return RedirectToAction(nameof(Index));
    }
}
