using CleanSolution.Core.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Core.Application.Validators
{
    public class SetPositionDtoValidator : AbstractValidator<SetPositionDto>
    {
        public SetPositionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("{PropertyName} მითითება აუცილებელია");

            RuleFor(x => x.Salary).NotEmpty().WithMessage("{PropertyName} მითითება აუცილებელია");
        }
    }
}
