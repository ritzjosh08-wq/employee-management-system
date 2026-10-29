namespace EmployeeManagement.Web.Models;

// Filters, sort order and paging for the employee list.
public record EmployeeQuery
{
    public string? Search { get; init; }
    public int? DepartmentId { get; init; }
    public bool? IsActive { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public static class EmployeeSort
{
    public const string Name = "name";
    public const string Department = "department";
    public const string JobTitle = "title";
    public const string Salary = "salary";
    public const string HireDate = "hired";
    public const string DescendingSuffix = "_desc";
}
