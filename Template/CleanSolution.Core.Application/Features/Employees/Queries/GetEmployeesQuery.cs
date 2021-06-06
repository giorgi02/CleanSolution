using AutoMapper;
using $safeprojectname$.DTOs;
using $safeprojectname$.Interfaces;
using CleanSolution.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace $safeprojectname$.Features.Employees.Queries
{
    public class GetEmployeesQuery
    {
        public class Request : IRequest<GetPaginationDto<GetEmployeeDto>>
        {
            public int PageIndex { get; set; }
            public int PageSize { get; set; }

            public string PrivateNumber { get; set; }
            public string FirstName { get; set; }
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
                var employees = await unit.EmployeeRepository.FilterAsync(request.PageIndex, request.PageSize, firatName: request.FirstName);

                return mapper.Map<GetPaginationDto<GetEmployeeDto>>(employees);
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1).WithMessage("მიუთითეთ გვერდის ნომერი");
                RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("მიუთითეთ გვერდის ზომა");
            }
        }
    }
}
