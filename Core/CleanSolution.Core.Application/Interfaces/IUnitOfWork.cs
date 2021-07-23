using CleanSolution.Core.Application.Interfaces.Repositories;

namespace CleanSolution.Core.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IPositionRepository PositionRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public ILogEventRepository LogObjectRepository { get; }
    }
}
