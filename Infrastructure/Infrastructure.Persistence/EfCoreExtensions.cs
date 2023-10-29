﻿using Core.Application.Commons;
using Core.Domain.Basics;

namespace Infrastructure.Persistence;
internal static class EfCoreExtensions
{
    public static async Task<Pagination<TEntity>> ToPaginatedAsync<TEntity>(this IQueryable<TEntity> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);

        return new Pagination<TEntity>(items, count, pageIndex, pageSize);
    }
}