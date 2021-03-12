using AutoMapper;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Domain.Entities;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Positions.Commands
{
    public class CreatePositionCommand
    {
        public class Request : IRequest
        {
            public string Name { get; set; }
            public double Salary { get; set; }
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
            public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var position = mapper.Map<Position>(request);
                unit.PositionRepository.Create(position);

                throw new NotImplementedException();
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("{PropertyName} მითითება აუცილებელია");

                RuleFor(x => x.Salary).NotEmpty().WithMessage("{PropertyName} მითითება აუცილებელია");
            }
        }
    }
}
