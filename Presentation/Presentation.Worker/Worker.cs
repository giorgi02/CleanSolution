using Core.Application.Interactors.Notifications;
using Cronos;
using MediatR;

namespace Presentation.Worker;
public class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly CronExpression _cron;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider services)
    {
        _services = services;
        _logger = services.GetRequiredService<ILogger<Worker>>();
        _cron = CronExpression.Parse("*/5 * * * *");
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(new UpsertPositionNotification.Request(), stoppingToken);

                var now = DateTimeOffset.Now;
                var next = _cron.GetNextOccurrence(now, TimeZoneInfo.Local)!.Value;
                var delay = next - now;

                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }
        }
    }
}