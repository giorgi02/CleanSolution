using CleanSolution.Core.Domain.Basics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace $safeprojectname$.Interfaces
{
    public interface IRepository<TKey, TEntity> where TEntity : BaseEntity
    {
        Task<int> CreateAsync(TEntity entity);

        Task<TEntity> ReadAsync(TKey id);
        Task<IEnumerable<TEntity>> ReadAsync();
        Task<IEnumerable<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateAsync(TKey id, TEntity entity);

        Task<int> DeleteAsync(TKey id);
        Task<int> DeleteAsync(TEntity entity);

        Task<bool> CheckAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
