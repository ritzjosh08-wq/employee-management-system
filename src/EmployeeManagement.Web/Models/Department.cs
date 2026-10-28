using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Web.Models;

public class Department
{
    public int DepartmentId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }

    // Calculated by the query, not stored in the Departments table.
    [Display(Name = "Employees")]
    public int EmployeeCount { get; set; }
}
