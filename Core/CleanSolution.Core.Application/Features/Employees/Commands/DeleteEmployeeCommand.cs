using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using Microsoft.Extensions.Localization;

namespace CleanSolution.Core.Application.Features.Employees.Commands;
public sealed class DeleteEmployeeCommand
{
    public record class Request(Guid EmployeeId) : IRequest;


    public class Handler : AsyncRequestHandler<Request>
    {
        private readonly IUnitOfWork _unit;
        private readonly IStringLocalizer<Localize.Resource> _localizer;

        public Handler(IUnitOfWork unit, IStringLocalizer<Localize.Resource> localizer)
        {
            _unit = unit;
            _localizer = localizer;
        }

        protected override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var isRecord = await _unit.EmployeeRepository.CheckAsync(x => x.Id == request.EmployeeId);

            cancellationToken.ThrowIfCancellationRequested();

            if (isRecord) throw new EntityNotFoundException(_localizer["exception_data_not_found"]);

            await _unit.EmployeeRepository.DeleteAsync(request.EmployeeId);
        }
    }
}