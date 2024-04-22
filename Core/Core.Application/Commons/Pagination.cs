using Microsoft.Extensions.Primitives;

namespace Core.Application.Commons;
public sealed class Pagination<T>
{
    public IEnumerable<T> Items { get; private set; }

    public long PageSize { get; private set; }
    public long PageIndex { get; private set; }

    public long TotalCount { get; private set; }
    public long TotalPages => (long)Math.Ceiling((double)this.TotalCount / this.PageSize);

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => this.PageIndex < this.TotalPages;




#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Pagination() { /* for deserialization "Mapster" */ }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Pagination(IEnumerable<T> items, long count, long pageIndex, long pageSize)
    {
        this.PageIndex = pageIndex;
        this.PageSize = pageSize;
        this.TotalCount = count;

        this.Items = items;
    }

    public Dictionary<string, StringValues> GetParams() => new()
    {
        [nameof(PageIndex)] = PageIndex.ToString(),
        [nameof(PageSize)] = PageSize.ToString(),

        [nameof(TotalPages)] = TotalPages.ToString(),
        [nameof(TotalCount)] = TotalCount.ToString(),

        [nameof(HasPreviousPage)] = HasPreviousPage.ToString(),
        [nameof(HasNextPage)] = HasNextPage.ToString(),
    };
}