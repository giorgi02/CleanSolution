using Core.Application.Commons;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Infrastructure.Persistence.Implementations;
internal sealed class EmployeeRepository : Repository<Guid, Employee>, IEmployeeRepository
{
    public EmployeeRepository(DataContext context) : base(context) { }

    private IQueryable<Employee> Including() => _context.Employes.Include(x => x.Position);


    public override async Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
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
            ).OrderByDescending(x => x.DateCreated)
            .ToPaginatedAsync(pageIndex, pageSize);

    public async Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text) =>
        await this.Including().Where(x => x.PrivateNumber == text || x.FirstName == text || x.LastName == text).ToPaginatedAsync(pageIndex, pageSize);

    public override async Task<Employee> UpdateAsync(Guid id, Employee employee, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Employes.FindAsync(id, cancellationToken);
        if (existing is null || existing.Version != employee.Version)
            throw new OperationForbiddenException("ასეთი ობიექტი ან არ არსებობს ან უკვე შეცვლილია");

        _context.Entry(existing).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}