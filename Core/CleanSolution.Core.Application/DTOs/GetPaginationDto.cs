using System.Collections.Generic;

namespace CleanSolution.Core.Application.DTOs
{
    public class GetPaginationDto<T>
    {
        public List<T> Items { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
