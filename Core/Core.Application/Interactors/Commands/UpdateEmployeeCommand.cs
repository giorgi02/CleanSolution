using Core.Application.DTOs;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Core.Application.Localize;
using Core.Domain.Enums;
using Core.Domain.Models;
using Mapster;
using Microsoft.Extensions.Localization;

namespace Core.Application.Interactors.Commands;
public abstract class UpdateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto>
    {
        internal Guid EmployeeId { get; private set; }
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
            Languages = new HashSet<Language>();
            Phones = Array.Empty<string>();
        }

        public Request SetEmployeeId(Guid employeeId)
        {
            EmployeeId = employeeId;
            return this;
        }
    }


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPositionRepository _positionRepository;

        public Handler(IEmployeeRepository employeeRepository, IPositionRepository positionRepository)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var position = await _positionRepository.ReadAsync(request.PositionId, cancellationToken);
            _ = position ?? throw new EntityNotFoundException("ზე ჩანაწერი ვერ მოიძებნა", nameof(request.PositionId));

            cancellationToken.ThrowIfCancellationRequested();

            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender)
            {
                Position = position,
                Version = request.Version
            };
            employee.SetLanguages(request.Languages);

            var result = await _employeeRepository.UpdateAsync(request.EmployeeId, employee, cancellationToken);

            return result.Adapt<GetEmployeeDto>();
        }
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly IPositionRepository _positionRepository;

        public Validator(IPositionRepository positionRepository, IStringLocalizer<Resource> localizer)
        {
            _positionRepository = positionRepository;

            RuleFor(x => x.PrivateNumber)
                .NotNull().WithMessage(localizer["field_is_empty", "PrivateNumber"])
                .Length(11).WithMessage(localizer["field_is_11_symbol", "PrivateNumber"])
                .Matches("^[0-9]*$").WithMessage(localizer["field_is_digits", "PrivateNumber"]);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("სახელის ველი ცარიელია");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("გვარის ველი ცარიელია");

            RuleFor(x => x.Gender).IsInEnum().WithMessage("მიუთითეთ სქესი სწორად");

            RuleFor(x => x.BirthDate)
                .Must(y => y < DateTime.Now).WithMessage("მიუთითეთ დაბადების თარიღი სწორად");

            //RuleFor(x => x.PositionId)
            //    .MustAsync(IfExistPosition).WithMessage("მიუთითეთ პოზიცია სწორად");
        }

        //private async Task<bool> IfExistPosition(Guid positionId, CancellationToken cancellationToken)
        //{
        //    return await _positionRepository.CheckAsync(x => x.Id == positionId);
        //}
    }
}