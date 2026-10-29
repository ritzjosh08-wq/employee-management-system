using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Web.Models;

public class Employee
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Range(0, 9_999_999_999.99)]
    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hire date")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    // Filled from a JOIN with Departments; not posted back by forms.
    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    [Display(Name = "Name")]
    public string FullName => $"{FirstName} {LastName}";

    // Shown in the avatar circle, e.g. "AT" for Ava Thompson.
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpperInvariant();
}
