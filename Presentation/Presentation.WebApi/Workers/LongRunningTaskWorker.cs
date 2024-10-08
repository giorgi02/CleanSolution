using Core.Application.DTOs;
using Core.Application.Interactors.Notifications;

namespace Presentation.WebApi.Workers;
public class LongRunningTaskWorker : BackgroundService
{
    private readonly IObservable<QueueItemDto> _stream;
    private readonly IServiceProvider _services;
    private IDisposable? _subscription;

    public LongRunningTaskWorker(IServiceProvider services)
    {
        _services = services;
        _stream = services.GetRequiredService<IObservable<QueueItemDto>>();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscription = _stream.Subscribe(async item => await ProcessItemAsync(item));
        return Task.CompletedTask;
    }

    private async Task ProcessItemAsync(QueueItemDto item)
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(new CheckoutProcessCommand.Request(item));
    }
}