using CleanSolution.Core.Application.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Queries
{
    public class GetEmployeesQuery
    {
        public class Request : IRequest<GetPaginationDto<GetEmployeeDto>> {
            public int pageIndex { get; set; }
            public int pageSize { get; set; }
        }
        public class Handler : IRequestHandler<Request, GetPaginationDto<GetEmployeeDto>>
        {
            public async Task<GetPaginationDto<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
