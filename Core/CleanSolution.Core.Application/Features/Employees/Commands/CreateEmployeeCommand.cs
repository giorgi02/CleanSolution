using AutoMapper;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Interfaces.Contracts;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Commands
{
    public class CreateEmployeeCommand
    {
        public class Request : IRequest<Guid>
        {
            public IFormFile Picture { get; set; }
            [Required]
            [StringLength(maximumLength: 11, MinimumLength = 11, ErrorMessage = "პირადი ნომერი უნდა შედგებოდეს 11 სიმბოლოსგან")]
            public string PrivateNumber { get; set; }
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
            public Gender Gender { get; set; }
            public ICollection<Language> Languages { get; set; }
            public DateTime? BirthDate { get; set; }
            public string[] Phones { get; set; }
            public Address Address { get; set; }
            public Guid PositionId { get; set; }

            public Request()
            {
                this.Languages = new HashSet<Language>();
            }
        }


        public class Handler : IRequestHandler<Request, Guid>
        {
            private readonly IUnitOfWork _unit;
            private readonly IFileManager _fileManager;
            private readonly IMapper _mapper;

            public Handler(IUnitOfWork unit, IFileManager fileManager, IMapper mapper)
            {
                _unit = unit;
                _fileManager = fileManager;
                _mapper = mapper;
            }
            public async Task<Guid> Handle(Request request, CancellationToken cancellationToken)
            {
                var employee = _mapper.Map<Employee>(request);
                employee.Position = await _unit.PositionRepository.ReadAsync(request.PositionId);

                cancellationToken.ThrowIfCancellationRequested();

                employee.PictureName = _fileManager.SaveFile(request.Picture);

                await _unit.EmployeeRepository.CreateAsync(employee);

                return employee.Id;
            }
        }


        public class Validator : AbstractValidator<Request>
        {
            private readonly IUnitOfWork _unit;

            public Validator(IUnitOfWork unit)
            {
                _unit = unit;

                RuleFor(x => x.PrivateNumber)
                    .NotNull().WithMessage("პირადი ნომერი ცარიელია")
                    //.Length(11).WithMessage("პირადი ნომერი უნდა შედგებოდეს 11 სიმბოლოსგან")
                    .Matches("^[0-9]*$").WithMessage("პირადი ნომერი უნდა შედგებოდეს მხოლოდ ციფრებისგან");

                RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("სახელის ველი ცარიელია");

                RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("გვარის ველი ცარიელია");

                RuleFor(x => x.Gender).IsInEnum().WithMessage("მიუთითეთ სქესი სწორად");

                RuleFor(x => x.BirthDate)
                    .Must(y => y < DateTime.Now).WithMessage("მიუთითეთ დაბადების თარიღი სწორად")
                    .Must(y => y > DateTime.Now.AddYears(-18) || y < DateTime.Now.AddYears(-100)).WithMessage("ამ ასაკის პიროვნება არ შეიძლება იყოს დასაქმებული");

                RuleFor(x => x.PositionId)
                    .MustAsync(IfExistPosition).WithMessage("მიუთითეთ პოზიცია სწორად");
            }

            private async Task<bool> IfExistPosition(Guid positionId, CancellationToken cancellationToken)
            {
                return await _unit.PositionRepository.CheckAsync(x => x.Id == positionId);
            }
        }
    }
}
