using Core.Application.Interfaces;
using Core.Domain.Basics;
using Core.Domain.Helpers;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;
internal abstract class Repository<TEntity> : IRepository<Guid, TEntity> where TEntity : BaseEntity
{
    protected readonly DataContext _context;
    public Repository(DataContext context) => _context = context;


    // სრული დაფარვა, ახდენს ყველა იმ კლასთან Include() რომელსაც კი შეიცავს მოცემული კლასი
    protected virtual IQueryable<TEntity> Including(IQueryable<TEntity> query)
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
    public virtual async Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Add(entity);
        return await _context.SaveChangesAsync(cancellationToken);
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
    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public virtual async Task<int> UpdateAsync(Guid id, TEntity entity, CancellationToken cancellationToken = default)
    {
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

    public async Task<IEnumerable<LogEvent>> GetAggregateEventsAsync(Guid id, int? version = null, DateTime? actTime = null)
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