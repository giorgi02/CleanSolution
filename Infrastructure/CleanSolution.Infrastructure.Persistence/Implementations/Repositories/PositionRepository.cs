using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;

namespace CleanSolution.Infrastructure.Persistence.Implementations.Repositories
{
    internal class PositionRepository : Repository<Position>, IPositionRepository
    {
        public PositionRepository(DataContext context) : base(context) { }
    }
}
