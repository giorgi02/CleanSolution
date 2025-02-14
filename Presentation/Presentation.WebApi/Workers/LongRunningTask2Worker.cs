using Core.Application.DTOs;
using Core.Application.Interactors.Notifications;
using System.Diagnostics;
using System.Threading.Channels;

namespace Presentation.WebApi.Workers;
public class LongRunningTask2Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly Channel<QueueItemDto> _channel;

    public LongRunningTask2Worker(IServiceProvider services, Channel<QueueItemDto> channel)
    {
        _services = services;
        _channel = channel;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            if (_channel.Reader.TryRead(out var item))
            {
                await ProcessItemAsync(item);
            }
        }
    }

    private async Task ProcessItemAsync(QueueItemDto item)
    {
        Debug.WriteLine("Run LongRunningTask2Worker");
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(new CheckoutProcessCommand.Request(item));
    }
}
