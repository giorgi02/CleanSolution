using AutoMapper;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Queries
{
    public class GetEmployeeHistoryQuery
    {
        public class Request : IRequest<GetEmployeeDto>
        {
            public Guid EmployeeId { get; set; }
            public int? Version { get; set; }
            public DateTime? ActTime { get; set; }

            public Request(Guid employeeId, int? version, DateTime? actTime)
            {
                EmployeeId = employeeId;
                Version = version;
                ActTime = actTime;
            }
        }

        public class Handler : IRequestHandler<Request, GetEmployeeDto>
        {
            private readonly IUnitOfWork unit;
            private readonly IMapper mapper;

            public Handler(IUnitOfWork unit, IMapper mapper)
            {
                this.unit = unit;
                this.mapper = mapper;
            }

            public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
            {
                var employee = await unit.EmployeeRepository.ReadAsync(request.EmployeeId);

                if (employee == null)
                    throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

                cancellationToken.ThrowIfCancellationRequested();

                var histroies = await unit.LogObjectRepository.GetEvents(request.EmployeeId, nameof(Employee), request.Version, request.ActTime);

                if (histroies == null)
                    return mapper.Map<GetEmployeeDto>(employee);

                cancellationToken.ThrowIfCancellationRequested();

                employee.Load(histroies.OrderByDescending(x => x.Version).ToList());

                return mapper.Map<GetEmployeeDto>(employee);
            }
        }
    }
}
