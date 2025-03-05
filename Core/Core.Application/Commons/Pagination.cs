﻿namespace Core.Application.Commons;
public sealed class Pagination<T>
{
    public PaginationMetaData Meta { get; private set; } = null!;
    public IEnumerable<T> Data { get; private set; } = null!;


    public static Pagination<T> Create(IEnumerable<T> data, long count, long pageIndex, long pageSize) => new()
    {
        Meta = new PaginationMetaData(count, pageIndex, pageSize),
        Data = data
    };
}

public sealed class PaginationMetaData
{
    public long PageSize { get; private set; }
    public long PageIndex { get; private set; }

    public long TotalCount { get; private set; }
    public long TotalPages => (long)Math.Ceiling((double)this.TotalCount / this.PageSize);

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => this.PageIndex < this.TotalPages;

    public PaginationMetaData(long count, long pageIndex, long pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = count;
    }
}