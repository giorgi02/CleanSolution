using CleanSolution.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CleanSolution.Infrastructure.Persistence.Implementations
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DataContext context;

        public Repository(DataContext context)
        {
            this.context = context;
        }


        public virtual void Create(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
        }


        public virtual TEntity Read(Guid id)
        {
            return context.Set<TEntity>().Find(id);
        }
        public virtual IEnumerable<TEntity> Read()
        {
            return context.Set<TEntity>();
        }
        public virtual IEnumerable<TEntity> Read(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate);
        }


        public virtual void Update(TEntity entity)
        {
            context.Set<TEntity>().Update(entity);
        }
        public virtual void Update(Guid id, TEntity entity)
        {
            var existing = context.Set<TEntity>().Find(id);
            this.context.Entry(existing).CurrentValues.SetValues(entity);
        }


        public virtual void Delete(TEntity entity)
        {
            context.Set<TEntity>().Remove(entity);
        }
        public virtual void Delete(Guid id)
        {
            context.Set<TEntity>().Remove(this.Read(id));
        }
    }
}
