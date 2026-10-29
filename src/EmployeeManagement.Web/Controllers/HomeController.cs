using System.Diagnostics;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repositories;
using EmployeeManagement.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;

    public HomeController(IEmployeeRepository employees, IDepartmentRepository departments)
    {
        _employees = employees;
        _departments = departments;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel
        {
            Stats = await _employees.GetStatsAsync(cancellationToken),
            Departments = await _departments.GetSummaryAsync(cancellationToken),
            RecentHires = await _employees.GetRecentHiresAsync(5, cancellationToken)
        };

        return View(model);
    }

    // Shown for 404s and other error status codes (wired up in Program.cs).
    [Route("Home/Status/{code:int}")]
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;
        return View(code);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
