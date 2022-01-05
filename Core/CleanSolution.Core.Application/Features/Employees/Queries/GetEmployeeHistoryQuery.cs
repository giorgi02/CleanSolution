using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;
using Microsoft.Extensions.Localization;

namespace CleanSolution.Core.Application.Features.Employees.Queries;
public sealed class GetEmployeeHistoryQuery
{
    public record class Request(Guid EmployeeId, int? Version, DateTime? ActTime)
        : IRequest<GetEmployeeDto>;


    public class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Localize.Resource> _localizer;

        public Handler(IUnitOfWork unit, IMapper mapper, IStringLocalizer<Localize.Resource> localizer)
        {
            _unit = unit;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = await _unit.EmployeeRepository.ReadAsync(request.EmployeeId);

            if (employee is null)
                throw new EntityNotFoundException(_localizer["record_not_found"]);

            cancellationToken.ThrowIfCancellationRequested();

            var histories = await _unit.EmployeeRepository.GetEventsAsync(request.EmployeeId, request.Version, request.ActTime);

            if (histories == null)
                return _mapper.Map<GetEmployeeDto>(employee);

            cancellationToken.ThrowIfCancellationRequested();

            employee.Load(histories.OrderByDescending(x => x.Version).ToList());

            return _mapper.Map<GetEmployeeDto>(employee);
        }
    }
}