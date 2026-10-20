using Core.Shared;
using Core.Shared.Extensions;

namespace Presentation.Worker;

/// <summary>
/// Cron-scheduled job (schedule in "CronExpression"). Each run gets a fresh DI scope from SlimTaskScheduler;
/// resolve the use-case handler from that scope and call it.
/// </summary>
public sealed class Worker(SlimTaskScheduler scheduler, IConfiguration configuration, ILogger<Worker> logger) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await scheduler.ExecutePeriodicallyAsync(async services =>
        {
            // e.g.:
            // var request = new SyncPositionsNotification.Request();
            // await services.ValidateOrThrowAsync(request, stoppingToken);
            // var handler = services.GetRequiredService<SyncPositionsNotification.Handler>();
            // await handler.Handle(request, stoppingToken);

            logger.LogInformation("Scheduled run at: {time}", DateTimeOffset.Now);
            await Task.CompletedTask;
        }, configuration.GetString("CronExpression"), stoppingToken);
    }
}
