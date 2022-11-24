using Core.Application.DTOs;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Mapster;

namespace Core.Application.Interactors.Employees.Queries;
public abstract class GetEmployeeQuery
{
    public record struct Request(Guid EmployeeId) : IRequest<GetEmployeeDto>;


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _repository;

        public Handler(IEmployeeRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var application = await _repository.ReadAsync(request.EmployeeId);
            if (application is null) throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

            return application.Adapt<GetEmployeeDto>();
        }
    }
}