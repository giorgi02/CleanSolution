using Core.Application.Interfaces.Repositories;
using Infrastructure.Persistence.Implementations.Repositories;

namespace Infrastructure.Persistence.Implementations;
[Obsolete("Clean Architecture მიდგომისთვის ვფიქრობ ეს პატერნი არ არის მიზანშეწონილი")]
internal class UnitOfWork : IDisposable
{
    private IPositionRepository? _positionRepository;
    private IEmployeeRepository? _employeeRepository;


    private readonly DataContext _context;
    public UnitOfWork(DataContext context) => _context = context;


    public IPositionRepository PositionRepository => _positionRepository ??= new PositionRepository(_context);
    public IEmployeeRepository EmployeeRepository => _employeeRepository ??= new EmployeeRepository(_context);


    public async Task CompleteAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}