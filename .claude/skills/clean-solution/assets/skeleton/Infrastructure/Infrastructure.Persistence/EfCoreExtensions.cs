using Core.Domain.Basics;
using Core.Shared;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

internal static class EfCoreExtensions
{
    /// <summary>
    /// 1-based paging: pageIndex = 1 is the first page.
    /// The query must already be ordered, otherwise SQL Server returns pages in an undefined order.
    /// </summary>
    public static async Task<Pagination<TEntity>> ToPaginatedAsync<TEntity>(this IQueryable<TEntity> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var count = await source.CountAsync(cancellationToken);
        if (count == 0) return Pagination<TEntity>.Create([], count, pageIndex, pageSize);

        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
        return Pagination<TEntity>.Create(items, count, pageIndex, pageSize);
    }

    /// <summary>
    /// Mapping shared by every AuditableEntity: optimistic concurrency on Version, audit column lengths,
    /// and the soft-delete query filter (rows with DeletedAt are invisible unless IgnoreQueryFilters() is used).
    /// Call it first in every IEntityTypeConfiguration of an AuditableEntity.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasQueryFilter(x => !x.DeletedAt.HasValue);

        return builder;
    }
}
