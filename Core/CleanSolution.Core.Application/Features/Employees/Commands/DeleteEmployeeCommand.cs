using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Resources;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Employees.Commands
{
    public class DeleteEmployeeCommand
    {
        public class Request : IRequest
        {
            public Guid EmployeeId { get; private set; }

            public Request(Guid employeeId) => this.EmployeeId = employeeId;
        }

        public class Handler : IRequestHandler<Request>
        {
            private readonly IUnitOfWork unit;

            public Handler(IUnitOfWork unit) =>
                this.unit = unit;

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var isRecord = await unit.EmployeeRepository.CheckAsync(x => x.Id == request.EmployeeId);

                if (isRecord) throw new EntityNotFoundException(Texts.exception_data_not_found);

                await unit.EmployeeRepository.DeleteAsync(request.EmployeeId);

                return Unit.Value;
            }
        }
    }
}
