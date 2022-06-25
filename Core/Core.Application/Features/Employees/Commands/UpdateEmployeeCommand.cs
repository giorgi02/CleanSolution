using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace Core.Application.Features.Employees.Commands;
public sealed class UpdateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto>
    {
        public Guid EmployeeId { get; private set; }
        public int Version { get; set; }

        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender Gender { get; set; }
        public ICollection<Language> Languages { get; set; }
        public DateTime BirthDate { get; set; }
        public string[] Phones { get; set; }
        public Address? Address { get; set; }
        public Guid PositionId { get; set; }


        public Request()
        {
            this.Languages = new HashSet<Language>();
            this.Phones = Array.Empty<string>();
        }

        public void SetId(Guid employeeId) => this.EmployeeId = employeeId;
    }


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IMapper _mapper;

        public Handler(IEmployeeRepository employeeRepository, IPositionRepository positionRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender)
            {
                Id = request.EmployeeId,
                Version = request.Version
            };
            employee.SetLanguages(request.Languages);
            employee.Position = await _positionRepository.ReadAsync(request.PositionId);

            cancellationToken.ThrowIfCancellationRequested();

            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return _mapper.Map<GetEmployeeDto>(employee);
        }
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly IPositionRepository _positionRepository;

        public Validator(IPositionRepository positionRepository, IStringLocalizer<Localize.Resource> localizer)
        {
            _positionRepository = positionRepository;

            RuleFor(x => x.PrivateNumber)
                .NotNull().WithMessage(localizer["validation_privatenumber_is_empty"])
                .Length(11).WithMessage(localizer["validation_privatenumber_is_11_symbol"])
                .Matches("^[0-9]*$").WithMessage(localizer["validation_privatenumber_is_digits"]);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("სახელის ველი ცარიელია");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("გვარის ველი ცარიელია");

            RuleFor(x => x.Gender).IsInEnum().WithMessage("მიუთითეთ სქესი სწორად");

            RuleFor(x => x.BirthDate)
                .Must(y => y < DateTime.Now).WithMessage("მიუთითეთ დაბადების თარიღი სწორად");

            RuleFor(x => x.PositionId)
                .MustAsync(IfExistPosition).WithMessage("მიუთითეთ პოზიცია სწორად");
        }

        private async Task<bool> IfExistPosition(Guid positionId, CancellationToken cancellationToken)
        {
            return await _positionRepository.CheckAsync(x => x.Id == positionId);
        }
    }
}