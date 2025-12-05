using Core.Domain.Basics;
using Core.Shared;

namespace Infrastructure.Persistence;

internal static class EfCoreExtensions
{
    public static async Task<Pagination<TEntity>> ToPaginatedAsync<TEntity>(this IQueryable<TEntity> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var count = await source.CountAsync(cancellationToken);
        if (count == 0) return Pagination<TEntity>.Create([], count, pageIndex, pageSize);

        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
        return Pagination<TEntity>.Create(items, count, pageIndex, pageSize);
    }
}