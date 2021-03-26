using AutoMapper;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Commands
{
    public class UpdateEmployeeCommand
    {
        public class Request : IRequest
        {
            public Guid EmployeeId { get; private set; }

            public string PrivateNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Gender Gender { get; set; }
            public DateTime BirthDate { get; set; }
            public string[] Phones { get; set; }
            public Address Address { get; set; }
            public Guid PositionId { get; set; }

            public void SetId(Guid employeeId) => this.EmployeeId = employeeId;
        }

        public class Handler : IRequestHandler<Request>
        {
            private readonly IUnitOfWork unit;
            private readonly IMapper mapper;

            public Handler(IUnitOfWork unit, IMapper mapper)
            {
                this.unit = unit;
                this.mapper = mapper;
            }
            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var employee = mapper.Map<Employee>(request);
                employee.Position = await unit.PositionRepository.ReadAsync(request.PositionId);

                await unit.EmployeeRepository.UpdateAsync(employee);

                return await Task.FromResult(Unit.Value);
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            private readonly IUnitOfWork unit;

            public Validator(IUnitOfWork unit)
            {
                this.unit = unit;

                RuleFor(x => x.PrivateNumber)
                    .NotNull().WithMessage("პირადი ნომერი ცარიელია")
                    .Length(11).WithMessage("პირადი ნომერი უნდა შედგებოდეს 11 სიმბოლოსგან")
                    .Matches("^[0-9]*$").WithMessage("პირადი ნომერი უნდა შედგებოდეს მხოლოდ ციფრებისგან");

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
                return await unit.PositionRepository.CheckAsync(x => x.Id == positionId);
            }
        }
    }
}
