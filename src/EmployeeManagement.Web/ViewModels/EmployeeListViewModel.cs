using EmployeeManagement.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.Web.ViewModels;

public class EmployeeListViewModel
{
    public required PagedResult<Employee> Result { get; init; }
    public required EmployeeQuery Query { get; init; }
    public required IEnumerable<SelectListItem> Departments { get; init; }

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Query.Search) || Query.DepartmentId is not null || Query.IsActive is not null;

    // The sort value a column header should link to: clicking the current
    // column again flips the direction.
    public string NextSort(string column) =>
        string.Equals(Query.Sort ?? EmployeeSort.Name, column, StringComparison.OrdinalIgnoreCase)
            ? column + EmployeeSort.DescendingSuffix
            : column;

    // "asc", "desc" or null, for the arrow next to a column header.
    public string? SortDirection(string column)
    {
        var sort = Query.Sort ?? EmployeeSort.Name;
        if (string.Equals(sort, column, StringComparison.OrdinalIgnoreCase)) return "asc";
        if (string.Equals(sort, column + EmployeeSort.DescendingSuffix, StringComparison.OrdinalIgnoreCase)) return "desc";
        return null;
    }
}
