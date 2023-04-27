using Microsoft.Extensions.Primitives;

namespace Core.Application.Commons;
public sealed class Pagination<T>
{
    public IEnumerable<T> Items { get; private set; }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }

    public long TotalPages { get; private set; }
    public long TotalCount { get; private set; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;




#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Pagination() { /* for deserialization "Mapster" */ }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Pagination(IEnumerable<T> items, long count, int pageIndex, int pageSize)
    {
        this.PageIndex = pageIndex;
        this.PageSize = pageSize;
        this.TotalPages = (long)Math.Ceiling(count / (double)pageSize);
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