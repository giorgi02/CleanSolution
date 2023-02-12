using Core.Domain.Basics;
using System.Linq.Expressions;

namespace Core.Application.Interfaces.Repositories;
public interface IRepository<TKey, TEntity> where TEntity : BaseEntity
{
    Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> ReadAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> ReadAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default);
    Task<int> UpdateSimpleAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> CheckAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
}