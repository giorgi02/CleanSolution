using Core.Application.Commons;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Interfaces.Repositories;
public interface IEmployeeRepository : IRepository<Guid, Employee>
{
    Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber = null, string? firatName = null, string? lastName = null, Gender? gender = null, Language? language = null);
    Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text);
}