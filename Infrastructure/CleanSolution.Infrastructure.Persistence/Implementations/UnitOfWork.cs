using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Infrastructure.Persistence.Implementations.Repositories;

namespace CleanSolution.Infrastructure.Persistence.Implementations;
internal class UnitOfWork : IUnitOfWork, IDisposable
{
    private IPositionRepository? _positionRepository;
    private IEmployeeRepository? _employeeRepository;


    private readonly DataContext _context;
    public UnitOfWork(DataContext context) => this._context = context;


    public IPositionRepository PositionRepository => _positionRepository ??= new PositionRepository(_context);
    public IEmployeeRepository EmployeeRepository => _employeeRepository ??= new EmployeeRepository(_context);


    public async Task CompleteAsync() => await _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}