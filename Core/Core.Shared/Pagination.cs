namespace Core.Shared;

public sealed class Pagination<T>
{
    public MetaData Meta { get; private set; } = null!;
    public IEnumerable<T> Data { get; private set; } = null!;


    public static Pagination<T> Create(IEnumerable<T> data, long count, long pageIndex, long pageSize) => new()
    {
        Meta = new MetaData(count, pageIndex, pageSize),
        Data = data
    };


    public sealed class MetaData
    {
        public long PageSize { get; private set; }
        public long PageIndex { get; private set; }

        public long TotalCount { get; private set; }
        public long TotalPages => (long)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public MetaData(long count, long pageIndex, long pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
        }
    }
}