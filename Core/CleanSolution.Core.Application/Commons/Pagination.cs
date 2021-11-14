namespace CleanSolution.Core.Application.Commons;
public class Pagination<T>
{
    public List<T> Items { get; }

    public int PageIndex { get; }
    public int PageSize { get; }

    public int TotalPages { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;


    public Pagination(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static Task<Pagination<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source.Take(((pageIndex - 1) * pageSize)..pageSize).ToList();

        return Task.Run(() => new Pagination<T>(items, count, pageIndex, pageSize));
    }
}