using Microsoft.Extensions.Primitives;

namespace CleanSolution.Core.Application.Commons;
public class Pagination<T>
{
    public List<T> Items { get; private set; }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }

    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;




#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Pagination() { /* for deserialization "AutoMapper" */ }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private Pagination(List<T> items, int count, int pageIndex, int pageSize)
    {
        this.PageIndex = pageIndex;
        this.PageSize = pageSize;
        this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        this.TotalCount = count;
        this.Items = items;
    }

    public static Task<Pagination<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        return Task.Run(() => new Pagination<T>(items, count, pageIndex, pageSize));
    }

    public Dictionary<string, StringValues> GetParams() => new()
    {
        { nameof(this.PageIndex), this.PageIndex.ToString() },
        { nameof(this.PageSize), this.PageSize.ToString() },

        { nameof(this.TotalPages), this.TotalPages.ToString() },
        { nameof(this.TotalCount), this.TotalCount.ToString() },

        { nameof(this.HasPreviousPage), this.HasPreviousPage.ToString() },
        { nameof(this.HasNextPage), this.HasNextPage.ToString() },
    };
}