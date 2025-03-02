using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Interactors.Commands;
public abstract class CreateTodoItemCommand
{
    public sealed record class Request : IRequest
    {
        public string? Title { get; set; }
        public string? Note { get; set; }
        public int Count { get; set; }
    }


    public sealed class Handler : IRequestHandler<Request>
    {
        private readonly ITodoItemRepository _repository;
        private readonly IMessagingService _messaging;

        public Handler(IServiceProvider services)
        {
            _repository = services.GetRequiredService<ITodoItemRepository>();
            _messaging = services.GetRequiredService<IMessagingService>();
        }

        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotNull().WithMessage("Title ცარიელია")
                .MinimumLength(11).WithMessage("Title უნდა შედგებოდეს ერთზე მეტი სიმბოლოსგან");
        }
    }
}