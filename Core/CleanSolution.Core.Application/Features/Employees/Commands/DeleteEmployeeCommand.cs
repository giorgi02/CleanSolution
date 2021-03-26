using CleanSolution.Core.Application.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Workabroad.Core.Application.Exceptions;

namespace CleanSolution.Core.Application.Features.Employees.Commands
{
    public class DeleteEmployeeCommand
    {
        public class Request : IRequest
        {
            public Guid EmploueeId { get; private set; }

            public Request(Guid employeeId) => this.EmploueeId = employeeId;
        }

        public class Handler : IRequestHandler<Request>
        {
            private readonly IUnitOfWork unit;

            public Handler(IUnitOfWork unit) =>
                this.unit = unit;

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var isRecord = await unit.EmployeeRepository.CheckAsync(x => x.Id == request.EmploueeId);
                if (isRecord)
                    throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

                await unit.EmployeeRepository.DeleteAsync(request.EmploueeId);

                return await Task.FromResult(Unit.Value);
            }
        }
    }
}
