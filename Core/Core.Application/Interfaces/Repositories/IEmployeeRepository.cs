using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Shared;
using System.Linq.Expressions;

namespace Core.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber = null, string? firatName = null, string? lastName = null, Gender? gender = null, Language language = Language.None);
    Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text);

    Task<Employee> UpdateAsync(Guid id, Employee employee, CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CheckAsync(Expression<Func<Employee, bool>> predicate);
}