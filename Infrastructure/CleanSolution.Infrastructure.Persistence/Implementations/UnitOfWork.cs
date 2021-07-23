using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Infrastructure.Persistence.Implementations.Repositories;

namespace CleanSolution.Infrastructure.Persistence.Implementations
{
    internal class UnitOfWork : IUnitOfWork
    {
        private IPositionRepository positionRepository;
        private IEmployeeRepository employeeRepository;
        private ILogEventRepository logObjectRepository;


        private readonly DataContext context;
        public UnitOfWork(DataContext context) => this.context = context;


        public IPositionRepository PositionRepository => positionRepository ??= new PositionRepository(context);
        public IEmployeeRepository EmployeeRepository => employeeRepository ??= new EmployeeRepository(context);
        public ILogEventRepository LogObjectRepository => logObjectRepository ??= new LogEventRepository(context);
    }
}
