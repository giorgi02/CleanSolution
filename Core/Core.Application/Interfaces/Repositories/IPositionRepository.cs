using Core.Domain.Models;
using System.Linq.Expressions;

namespace Core.Application.Interfaces.Repositories;

public interface IPositionRepository
{
    Task<Position?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Position>> ReadAsync(CancellationToken cancellationToken = default);

    Task<bool> CheckAsync(Expression<Func<Position, bool>> predicate);
}