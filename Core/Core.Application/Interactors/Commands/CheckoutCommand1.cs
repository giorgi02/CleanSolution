using Core.Application.DTOs;

namespace Core.Application.Interactors.Commands;
public abstract class CheckoutCommand1
{
    public record struct Request(int ItemsCount, decimal Amount) : IRequest<Response>;


    public sealed class Handler(IObserver<QueueItemDto> stream) : IRequestHandler<Request, Response>
    {
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
                Text = $"შეკვეთა {request.ItemsCount} {request.Amount}"
            };

            stream.OnNext(queueItem);

            return Task.FromResult(response);
        }
    }


    public sealed record class Response
    {
        public Guid OrderId { get; set; }
        public required string Message { get; set; }
    }
}