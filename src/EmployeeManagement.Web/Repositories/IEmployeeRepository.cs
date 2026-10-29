using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Repositories;

public interface IEmployeeRepository
{
    Task<PagedResult<Employee>> SearchAsync(EmployeeQuery query, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetRecentHiresAsync(int count, CancellationToken cancellationToken = default);
    Task<EmployeeStats> GetStatsAsync(CancellationToken cancellationToken = default);
}
