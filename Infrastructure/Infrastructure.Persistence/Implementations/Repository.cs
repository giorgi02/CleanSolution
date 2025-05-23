﻿using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Basics;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;
[Obsolete("შედარებით რთული პროექტებისთვის ვფიქრობ ეს პატერნი გამოუსადეგარია")]
internal abstract class Repository<TKey, TEntity> : IRepository<TKey, TEntity>
    where TKey : struct
    where TEntity : BaseEntity
{
    protected readonly DataContext _context;
    public Repository(DataContext context) => _context = context;


    // create
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var res = _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return res.Entity;
    }
    // read
    public virtual async Task<TEntity?> ReadAsync(TKey id, CancellationToken cancellationToken = default)
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
    public virtual async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var res = _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return res.Entity;
    }
    public virtual async Task<TEntity> UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.Id = default;
        var existing = await _context.Set<TEntity>().FindAsync(id, cancellationToken)
            ?? throw OperationForbiddenException.Create("ასეთი ობიექტი ან არ არსებობს ან უკვე შეცვლილია");

        _context.Entry(existing).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }
    // todo: შევამოწმო ეს მეთოდი
    public virtual async Task<TEntity?> UpdateSimpleAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    // delete
    public virtual async Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var item = await this.ReadAsync(id, cancellationToken);
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