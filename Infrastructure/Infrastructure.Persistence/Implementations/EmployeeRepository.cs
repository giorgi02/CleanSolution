using Core.Application.Commons;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Infrastructure.Persistence.Implementations;
internal sealed class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(DataContext context) : base(context) { }


    public IQueryable<Employee> Including() =>
        _context.Employes.Include(x => x.Position);


    public override async Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Including().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber = null, string? firstName = null, string? lastName = null, Gender? gender = null, Language? language = null)
    {
        var employees = Including().Where(x =>
            (privateNumber == null || x.PrivateNumber == privateNumber) &&
            (firstName == null || x.FirstName == firstName) &&
            (lastName == null || x.LastName == lastName) &&
            (gender == null || x.Gender == gender) &&
            (language == null || x.Language.HasFlag(language))
        );

        return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
    }

    public async Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text)
    {
        var employees = Including().Where(x => x.PrivateNumber == text || x.FirstName == text || x.LastName == text);

        return await Pagination<Employee>.CreateAsync(employees, pageIndex, pageSize);
    }

    public override async Task<int> UpdateAsync(Guid id, Employee employee, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Employes.FindAsync(id);
        if (existing is null || existing.Version != employee.Version)
            throw new DataObsoleteException("ასეთი ობიექტი ან არ არსებობს ან უკვე შეცვლილია");

        _context.Entry(existing).CurrentValues.SetValues(employee);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}