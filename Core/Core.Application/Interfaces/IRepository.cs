using Core.Domain.Basics;
using Core.Domain.Helpers;
using System.Linq.Expressions;

namespace Core.Application.Interfaces;
public interface IRepository<TKey, TEntity> where TEntity : BaseEntity
{
    Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> ReadAsync(TKey id);
    Task<IEnumerable<TEntity>> ReadAsync();
    Task<IEnumerable<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default);
    Task<int> UpdateSimpleAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> CheckAsync(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    Task<IEnumerable<LogEvent>> GetAggregateEventsAsync(TKey id, int? version = null, DateTime? actTime = null);


    // todo: კეთდება C# ის ახალი ფუნქციონალით, შვილით გადატვირთვა
    object Test();
}