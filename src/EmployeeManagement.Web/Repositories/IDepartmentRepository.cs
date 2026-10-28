using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Repositories;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Department department, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Department department, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentSummary>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
