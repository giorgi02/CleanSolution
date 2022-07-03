using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Microsoft.Extensions.Localization;

namespace Core.Application.Features.Employees.Commands;
public sealed class DeleteEmployeeCommand
{
    public record struct Request(Guid EmployeeId) : IRequest;


    public sealed class Handler : AsyncRequestHandler<Request>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IStringLocalizer<Localize.Resource> _localizer;

        public Handler(IEmployeeRepository employeeRepository, IStringLocalizer<Localize.Resource> localizer)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        protected override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var isRecord = await _employeeRepository.CheckAsync(x => x.Id == request.EmployeeId);

            cancellationToken.ThrowIfCancellationRequested();

            if (!isRecord) throw new EntityNotFoundException(_localizer["record_not_found"]);

            await _employeeRepository.DeleteAsync(request.EmployeeId, cancellationToken);
        }
    }
}