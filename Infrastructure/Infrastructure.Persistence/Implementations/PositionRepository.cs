using Core.Application.Interfaces.Repositories;
using Core.Domain.Models;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;

internal sealed class PositionRepository : IPositionRepository
{
    private readonly DataContext _context;
    public PositionRepository(DataContext context) => _context = context;


    public async Task<Position?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Positions.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<Position>> ReadAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Positions.ToListAsync(cancellationToken);
    }

    public async Task<bool> CheckAsync(Expression<Func<Position, bool>> predicate)
    {
        return await _context.Positions.AnyAsync(predicate);
    }
}