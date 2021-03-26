using AutoMapper;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Queries
{
    public class GetEmployeesQuery
    {
        public class Request : IRequest<GetPaginationDto<GetEmployeeDto>>
        {
            public int pageIndex { get; set; }
            public int pageSize { get; set; }

            public string PrivateNumber { get; set; }
            public string FiratName { get; set; }
            public string LastName { get; set; }
            public Gender? Gender { get; set; }
        }

        public class Handler : IRequestHandler<Request, GetPaginationDto<GetEmployeeDto>>
        {
            private readonly IUnitOfWork unit;
            private readonly IMapper mapper;

            public Handler(IUnitOfWork unit, IMapper mapper)
            {
                this.unit = unit;
                this.mapper = mapper;
            }

            public async Task<GetPaginationDto<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
            {
                var employees = await unit.EmployeeRepository.FilterAsync(request.pageIndex, request.pageSize, firatName: request.FiratName);

                return mapper.Map<GetPaginationDto<GetEmployeeDto>>(employees);
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(1).WithMessage("მიუთითეთ გვერდის ნომერი");
                RuleFor(x => x.pageSize).GreaterThan(0).WithMessage("მიუთითეთ გვერდის ზომა");
            }
        }
    }
}
