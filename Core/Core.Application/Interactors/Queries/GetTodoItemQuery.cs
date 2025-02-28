using Core.Application.DTOs;
using Core.Application.Exceptions;
using Core.Application.Interfaces.Repositories;
using Mapster;

namespace Core.Application.Interactors.Queries;
public abstract class GetTodoItemQuery
{
    public record struct Request(Guid Id) : IRequest<GetTodoItemDto>;


    public sealed class Handler : IRequestHandler<Request, GetTodoItemDto>
    {
        private readonly ITodoItemRepository _repository;
        public Handler(ITodoItemRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));


        public async Task<GetTodoItemDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var application = await _repository.GetAsync(request.Id)
                ?? throw EntityNotFoundException.Create("ჩანაწერი ვერ მოიძებნა");

            return application.Adapt<GetTodoItemDto>();
        }
    }
}