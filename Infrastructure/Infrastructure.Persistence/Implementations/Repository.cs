using Core.Application.Interfaces.Repositories;
using Core.Domain.Basics;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;
internal abstract class Repository<TEntity> : IRepository<Guid, TEntity> where TEntity : BaseEntity
{
    protected readonly DataContext _context;
    public Repository(DataContext context) => _context = context;


    // create
    public virtual async Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Add(entity);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    // read
    public virtual async Task<TEntity?> ReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FindAsync(id, cancellationToken);
    }
    public virtual async Task<IEnumerable<TEntity>> ReadAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().ToListAsync(cancellationToken);
    }
    public virtual async Task<IEnumerable<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }
    // update
    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task<int> UpdateAsync(Guid id, TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.Id = id;
        var existing = await _context.Set<TEntity>().FindAsync(id);
        if (existing is null) return 0;

        _context.Entry(existing).CurrentValues.SetValues(entity);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    // todo: შევამოწმო ეს მეთოდი
    public virtual async Task<int> UpdateSimpleAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return await _context.SaveChangesAsync(cancellationToken);
    }
    // delete
    public virtual async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await this.ReadAsync(id);
        if (item is null) return 0;

        _context.Set<TEntity>().Remove(item);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Remove(entity);
        return await _context.SaveChangesAsync(cancellationToken);
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
}