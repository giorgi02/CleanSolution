using Microsoft.Extensions.Primitives;

namespace Core.Application.Commons;
public sealed class Pagination<T>
{
    public IEnumerable<T> Items { get; private set; } = null!;

    public long PageSize { get; private set; }
    public long PageIndex { get; private set; }

    public long TotalCount { get; private set; }
    public long TotalPages => (long)Math.Ceiling((double)this.TotalCount / this.PageSize);

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => this.PageIndex < this.TotalPages;


    public static Pagination<T> Create(IEnumerable<T> items, long count, long pageIndex, long pageSize) => new()
    {
        PageIndex = pageIndex,
        PageSize = pageSize,
        TotalCount = count,

        Items = items
    };

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