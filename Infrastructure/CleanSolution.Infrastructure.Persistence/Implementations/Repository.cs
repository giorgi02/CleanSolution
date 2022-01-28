﻿using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Basics;
using CleanSolution.Core.Domain.Helpers;
using System.Linq.Expressions;

namespace CleanSolution.Infrastructure.Persistence.Implementations;
internal abstract class Repository<TEntity> : IRepository<Guid, TEntity> where TEntity : BaseEntity
{
    protected readonly DataContext _context;
    public Repository(DataContext context) => _context = context;


    // სრული დაფარვა, ახდენს ყველა იმ კლასთან Include() რომელსაც კი შეიცავს მოცემული კლასი
    public IQueryable<TEntity> Including<TEntity>(IQueryable<TEntity> query) where TEntity : class
    {
        foreach (var item in query)
        {
            foreach (var property in item.GetType().GetProperties())
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    query = query.Include(property.Name);
            break;
        }

        return query;
    }

    // create
    public virtual async Task<int> CreateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
        return await _context.SaveChangesAsync();
    }
    // read
    public virtual async Task<IEnumerable<TEntity>> ReadAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }
    public virtual async Task<TEntity?> ReadAsync(Guid id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }
    public virtual async Task<IEnumerable<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync();
    }
    // update
    public virtual async Task<int> UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        return await _context.SaveChangesAsync();
    }
    public virtual async Task<int> UpdateAsync(Guid id, TEntity entity)
    {
        var existing = await _context.Set<TEntity>().FindAsync(id);
        if (existing is null) return 0;

        _context.Entry(existing).CurrentValues.SetValues(entity);
        return await _context.SaveChangesAsync();
    }
    // todo: შევამოწმო ეს მეთოდი
    public virtual async Task<int> UpdateSimpleAsync(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return await _context.SaveChangesAsync();
    }
    // delete
    public virtual async Task<int> DeleteAsync(Guid id)
    {
        var item = await this.ReadAsync(id);
        if (item is null) return 0;

        _context.Set<TEntity>().Remove(item);
        return await _context.SaveChangesAsync();
    }
    public virtual async Task<int> DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return await _context.SaveChangesAsync();
    }
    // check
    public virtual async Task<bool> CheckAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().CountAsync(predicate);
    }

    public async Task<IEnumerable<LogEvent>> GetEventsAsync(Guid id, int? version = null, DateTime? actTime = null)
    {
        return await _context.LogEvents
            .Where(x => x.ObjectId == id &&
                x.ObjectType == typeof(TEntity).Name &&
                (version == null || x.Version >= version) &&
                (actTime == null || x.ActTime >= actTime))
            .ToListAsync();
    }

    // todo: კეთდება C# ის ახალი ფუნქციონალით, შვილით გადატვირთვა
    public virtual object Test()
    {
        throw new NotImplementedException();
    }
}