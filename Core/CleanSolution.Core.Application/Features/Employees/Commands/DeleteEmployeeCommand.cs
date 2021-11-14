using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Resources;
using MediatR;

namespace CleanSolution.Core.Application.Features.Employees.Commands;
public class DeleteEmployeeCommand
{
    public record Request(Guid EmployeeId) : IRequest;


    public class Handler : IRequestHandler<Request>
    {
        private readonly IUnitOfWork _unit;
        public Handler(IUnitOfWork unit) => _unit = unit;

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var isRecord = await _unit.EmployeeRepository.CheckAsync(x => x.Id == request.EmployeeId);

            if (isRecord) throw new EntityNotFoundException(text_exceptions.exception_data_not_found);

            await _unit.EmployeeRepository.DeleteAsync(request.EmployeeId);

            return Unit.Value;
        }
    }
}