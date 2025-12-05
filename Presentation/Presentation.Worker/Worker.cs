using Core.Application.Interactors.Notifications;
using Core.Shared;

namespace Presentation.Worker;

public class Worker : BackgroundService
{
    private readonly SlimTaskScheduler _slimTaskScheduler;
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(IServiceProvider services)
    {
        _slimTaskScheduler = services.GetRequiredService<SlimTaskScheduler>();
        _logger = services.GetRequiredService<ILogger<Worker>>();
        _configuration = services.GetRequiredService<IConfiguration>();
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _slimTaskScheduler.ExecutePeriodicallyAsync(async serviceProvider =>
        {
            var request = new UpsertPositionNotification.Request();
            var handler = serviceProvider.GetRequiredService<UpsertPositionNotification.Handler>();
            await handler.Handle(request, stoppingToken);
        }, _configuration.GetString("CrosExpression"), stoppingToken);
    }
}