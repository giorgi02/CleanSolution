using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces;

namespace CleanSolution.Core.Application.Features.Employees.Queries;
public sealed class GetEmployeeQuery
{
    public record Request(Guid EmployeeId) : IRequest<GetEmployeeDto>;


    public class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public Handler(IUnitOfWork unit, IMapper mapper)
        {
            _unit = unit;
            _mapper = mapper;
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var application = await _unit.EmployeeRepository.ReadAsync(request.EmployeeId);
            if (application is null) throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

            return await Task.FromResult(_mapper.Map<GetEmployeeDto>(application));
        }
    }
}