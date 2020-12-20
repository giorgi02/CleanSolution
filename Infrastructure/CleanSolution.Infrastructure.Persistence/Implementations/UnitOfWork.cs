using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Infrastructure.Persistence.Implementations.Repositories;

namespace CleanSolution.Infrastructure.Persistence.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private IPositionRepository positionRepository;
        private IEmployeeRepository employeeRepository;


        private readonly DataContext context;
        public UnitOfWork(DataContext context)
        {
            this.context = context;
        }


        public IPositionRepository PositionRepository { get { return positionRepository ??= new PositionRepository(context); } }
        public IEmployeeRepository EmployeeRepository { get { return employeeRepository ??= new EmployeeRepository(context); } }
    }
}
