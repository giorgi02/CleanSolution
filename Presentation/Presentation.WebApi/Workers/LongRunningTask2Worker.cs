using Core.Application.Interactors.Notifications;
using System.Diagnostics;
using System.Threading.Channels;

namespace Presentation.WebApi.Workers;

public class LongRunningTask2Worker(IServiceProvider services, Channel<QueueItemDto> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await channel.Reader.WaitToReadAsync(stoppingToken))
        {
            if (channel.Reader.TryRead(out var item))
                await ProcessItemAsync(item);
        }
    }

    private async Task ProcessItemAsync(QueueItemDto item)
    {
        Debug.WriteLine("Run LongRunningTask2Worker");
        using var scope = services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(new CheckoutProcessCommand.Request(item));
    }
}
