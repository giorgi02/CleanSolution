using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;

namespace Infrastructure.Persistence.Implementations.Repositories;
internal sealed class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(DataContext context) : base(context) { }
}