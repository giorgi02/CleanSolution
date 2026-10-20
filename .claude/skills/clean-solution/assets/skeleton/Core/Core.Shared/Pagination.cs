namespace Core.Shared;

public sealed class Pagination<T>
{
    public MetaData Meta { get; private set; } = null!;
    public IEnumerable<T> Data { get; private set; } = null!;


    public static Pagination<T> Create(IEnumerable<T> data, long totalCount, long pageIndex, long pageSize) => new()
    {
        Meta = new MetaData(totalCount, pageIndex, pageSize),
        Data = data
    };


    // constructor parameter names must match the property names: Mapster maps
    // Pagination<TEntity> => Pagination<TDto> through this constructor (MapToConstructor)
    public sealed class MetaData(long totalCount, long pageIndex, long pageSize)
    {
        public long PageSize { get; } = pageSize;
        public long PageIndex { get; } = pageIndex;

        public long TotalCount { get; } = totalCount;
        public long TotalPages => PageSize == 0 ? 0 : (long)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
