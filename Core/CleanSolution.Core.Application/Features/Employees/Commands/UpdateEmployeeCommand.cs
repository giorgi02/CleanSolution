﻿using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace CleanSolution.Core.Application.Features.Employees.Commands;
public sealed class UpdateEmployeeCommand
{
    public class Request : IRequest<GetEmployeeDto>
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


    public class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public Handler(IUnitOfWork unit, IMapper mapper)
        {
            _unit = unit;
            _mapper = mapper;
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender)
            {
                Id = request.EmployeeId,
                Version = request.Version
            };
            employee.SetLanguages(request.Languages);
            employee.Position = await _unit.PositionRepository.ReadAsync(request.PositionId);

            cancellationToken.ThrowIfCancellationRequested();

            await _unit.EmployeeRepository.UpdateAsync(employee);

            return _mapper.Map<GetEmployeeDto>(employee);
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IUnitOfWork _unit;

        public Validator(IUnitOfWork unit, IStringLocalizer<Localize.Resource> localizer)
        {
            _unit = unit;

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
            return await _unit.PositionRepository.CheckAsync(x => x.Id == positionId);
        }
    }
}