using $safeprojectname$.Interfaces.Repositories;

namespace $safeprojectname$.Interfaces
{
    public interface IUnitOfWork
    {
        public IPositionRepository PositionRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
    }
}
