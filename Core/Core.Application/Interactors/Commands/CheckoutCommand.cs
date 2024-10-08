using Core.Application.DTOs;

namespace Core.Application.Interactors.Commands;
public abstract class CheckoutCommand
{
    public record struct Request(int itemsCount, decimal amount) : IRequest<Response>;


    public sealed class Handler : IRequestHandler<Request, Response>
    {
        private readonly IObserver<QueueItemDto> _stream;

        public Handler(IObserver<QueueItemDto> stream)
        {
            _stream = stream;
        }

        public Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var response = new Response
            {
                OrderId = Guid.NewGuid(),
                Message = "თქვენი შეკვეთა მუშავდება. დასრულების შემდეგ მიიღებთ ყველა დეტალს email ზე"
            };

            var queueItem = new QueueItemDto
            {
                OrderId = response.OrderId,
                Text = $"შეკვეთა {request.itemsCount} {request.amount}"
            };

            _stream.OnNext(queueItem);

            return Task.FromResult(response);
        }
    }


    public sealed record class Response
    {
        public Guid OrderId { get; set; }
        public required string Message { get; set; }
    }
}