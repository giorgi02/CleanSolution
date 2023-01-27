using Core.Application.Interfaces.Repositories;
using Core.Domain.Models;

namespace Infrastructure.Persistence.Implementations;
internal sealed class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(DataContext context) : base(context) { }
}