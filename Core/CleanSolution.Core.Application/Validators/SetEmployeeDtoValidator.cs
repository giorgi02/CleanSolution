using CleanSolution.Core.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Core.Application.Validators
{
    public class SetEmployeeDtoValidator : AbstractValidator<SetEmployeeDto>
    {
        public SetEmployeeDtoValidator()
        {
            RuleFor(x => x.PrivateNumber)
                .NotEmpty().WithMessage("{PropertyName} ის მითითება აუცილებელია")
                .Length(11).WithMessage("{PropertyName} უნდა შედგებოდეს 11 სიმბოლოსგან");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("{PropertyName} ის მითითება აუცილებელია");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("{PropertyName} ის მითითება აუცილებელია");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("მიუთითეთ სქესი");

            RuleFor(x => x.BirthDate)
                .NotNull().WithMessage("მიუთითეთ დაბადების თარიღი");

        }
    }
}
