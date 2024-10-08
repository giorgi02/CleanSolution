using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;
using Mapster;

namespace Core.Application.Interactors.Queries;
public abstract class GetEmployeesQuery
{
    public sealed record class Request : IRequest<Pagination<GetEmployeeDto>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public ICollection<Language> Languages { get; set; }


        public Request() => Languages = new HashSet<Language>();
    }


    public sealed class Handler : IRequestHandler<Request, Pagination<GetEmployeeDto>>
    {
        private readonly IEmployeeRepository _repository;
        public Handler(IEmployeeRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Pagination<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            Language languages = Language.None;
            foreach (var language in request.Languages)
                languages |= language;

            var employees = await _repository.FilterAsync(request.PageIndex, request.PageSize, firatName: request.FirstName, language: languages);

            return employees.Adapt<Pagination<GetEmployeeDto>>();
        }
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1).WithMessage("მიუთითეთ გვერდის ნომერი");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("მიუთითეთ გვერდის ზომა");
        }
    }
}