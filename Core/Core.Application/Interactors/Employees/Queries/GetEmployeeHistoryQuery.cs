using Core.Application.DTOs;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Core.Application.Localize;
using Mapster;
using Microsoft.Extensions.Localization;

namespace Core.Application.Interactors.Employees.Queries;
public abstract class GetEmployeeHistoryQuery
{
    public record struct Request(Guid EmployeeId, int? Version, DateTime? ActTime) : IRequest<GetEmployeeDto>;


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _repository;
        private readonly IStringLocalizer<Resource> _localizer;

        public Handler(IEmployeeRepository repository, IStringLocalizer<Resource> localizer)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = await _repository.ReadAsync(request.EmployeeId);

            if (employee is null)
                throw new EntityNotFoundException(_localizer["record_not_found"]);

            cancellationToken.ThrowIfCancellationRequested();

            var histories = await _repository.GetAggregateEventsAsync(request.EmployeeId, request.Version, request.ActTime);

            if (histories == null)
                return employee.Adapt<GetEmployeeDto>();

            cancellationToken.ThrowIfCancellationRequested();

            employee.Load(histories.OrderByDescending(x => x.Version).ToList());

            return employee.Adapt<GetEmployeeDto>();
        }
    }
}