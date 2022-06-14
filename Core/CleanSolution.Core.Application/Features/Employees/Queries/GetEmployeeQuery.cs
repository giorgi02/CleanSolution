using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Exceptions;
using CleanSolution.Core.Application.Interfaces.Repositories;

namespace CleanSolution.Core.Application.Features.Employees.Queries;
public sealed class GetEmployeeQuery
{
    public sealed record class Request(Guid EmployeeId) : IRequest<GetEmployeeDto>;


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;

        public Handler(IEmployeeRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var application = await _repository.ReadAsync(request.EmployeeId);
            if (application is null) throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

            return await Task.FromResult(_mapper.Map<GetEmployeeDto>(application));
        }
    }
}