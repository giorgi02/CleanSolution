using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Enums;

namespace CleanSolution.Core.Application.Features.Employees.Queries;
public sealed class GetEmployeesQuery
{
    public class Request : IRequest<Pagination<GetEmployeeDto>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public ICollection<Language> Languages { get; set; }


        public Request() => this.Languages = new HashSet<Language>();
    }


    public class Handler : IRequestHandler<Request, Pagination<GetEmployeeDto>>
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public Handler(IUnitOfWork unit, IMapper mapper)
        {
            _unit = unit;
            _mapper = mapper;
        }

        public async Task<Pagination<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            Language languages = Language.None;
            foreach (var language in request.Languages)
                languages |= language;

            var employees = await _unit.EmployeeRepository.FilterAsync(request.PageIndex, request.PageSize, firatName: request.FirstName, language: languages);

            return _mapper.Map<Pagination<GetEmployeeDto>>(employees);
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