using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CleanSolution.Core.Application.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Create(TEntity entity);

        TEntity Read(Guid id);
        IEnumerable<TEntity> Read();
        IEnumerable<TEntity> Read(Expression<Func<TEntity, bool>> predicate);

        void Update(TEntity entity);
        void Update(Guid id, TEntity entity);

        void Delete(TEntity entity);
        void Delete(Guid id);
    }
}
