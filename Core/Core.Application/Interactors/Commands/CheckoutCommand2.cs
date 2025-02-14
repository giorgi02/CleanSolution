using Core.Application.DTOs;
using System.Threading.Channels;

namespace Core.Application.Interactors.Commands;
public abstract class CheckoutCommand2
{
    public record struct Request(int ItemsCount, decimal Amount) : IRequest<Response>;


    public sealed class Handler(Channel<QueueItemDto> channel) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
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

            await channel.Writer.WriteAsync(queueItem, cancellationToken);

            return response;
        }
    }


    public sealed record class Response
    {
        public Guid OrderId { get; set; }
        public required string Message { get; set; }
    }
}
