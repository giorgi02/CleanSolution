using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Mapster;

namespace Core.Application.Interactors.Queries;
public abstract class GetTodoItemsQuery
{
    public sealed record class Request : IRequest<Pagination<GetTodoItemDto>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string? Text { get; set; }
    }


    public sealed class Handler : IRequestHandler<Request, Pagination<GetTodoItemDto>>
    {
        private readonly ITodoItemRepository _repository;
        public Handler(ITodoItemRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Pagination<GetTodoItemDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var result = await _repository.SearchAsync(request.PageIndex, request.PageSize, request.Text);

            return result.Adapt<Pagination<GetTodoItemDto>>();
        }
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1).WithMessage("მიუთითეთ გვერდის ნომერი");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("მიუთითეთ გვერდის ზომა");
        }
    }
}