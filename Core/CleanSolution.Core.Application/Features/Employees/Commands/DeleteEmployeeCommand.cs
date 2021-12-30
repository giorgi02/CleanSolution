using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Resources;

namespace CleanSolution.Core.Application.Features.Employees.Commands;
public sealed class DeleteEmployeeCommand
{
    public record class Request(Guid EmployeeId) : IRequest;


    public class Handler : IRequestHandler<Request>
    {
        private readonly IUnitOfWork _unit;
        public Handler(IUnitOfWork unit) => _unit = unit;

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var isRecord = await _unit.EmployeeRepository.CheckAsync(x => x.Id == request.EmployeeId);

            cancellationToken.ThrowIfCancellationRequested();

            if (isRecord) throw new EntityNotFoundException(text_exceptions.exception_data_not_found);

            await _unit.EmployeeRepository.DeleteAsync(request.EmployeeId);

            return Unit.Value;
        }
    }
}