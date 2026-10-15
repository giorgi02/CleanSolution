using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Shared;
using Core.Shared.Exceptions;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;

internal sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly DataContext _context;
    public EmployeeRepository(DataContext context) => _context = context;

    private IQueryable<Employee> Including() => _context.Employees.Include(x => x.Position);


    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var res = _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return res.Entity;
    }

    public async Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Including().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber = null, string? firstName = null, string? lastName = null, Gender? gender = null, Language language = Language.None) =>
         await this.Including().Where(x =>
                (privateNumber == null || x.PrivateNumber == privateNumber) &&
                (firstName == null || x.FirstName == firstName) &&
                (lastName == null || x.LastName == lastName) &&
                (gender == null || x.Gender == gender) &&
                (language == Language.None || x.Language.HasFlag(language))
            ).OrderByDescending(x => x.CreatedAt)
            .ToPaginatedAsync(pageIndex, pageSize);

    public async Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text) =>
        await this.Including().Where(x => x.PrivateNumber == text || x.FirstName == text || x.LastName == text).ToPaginatedAsync(pageIndex, pageSize);

    public async Task<Employee> UpdateAsync(Guid id, Employee employee, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Employees.FindAsync([id], cancellationToken);
        if (existing is null || existing.Version != employee.Version)
            throw OperationForbiddenException.Create("ასეთი ობიექტი ან არ არსებობს ან უკვე შეცვლილია");

        _context.Entry(existing).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await ReadAsync(id, cancellationToken);
        if (item is null) return 0;

        _context.Employees.Remove(item);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CheckAsync(Expression<Func<Employee, bool>> predicate)
    {
        return await _context.Employees.AnyAsync(predicate);
    }
}