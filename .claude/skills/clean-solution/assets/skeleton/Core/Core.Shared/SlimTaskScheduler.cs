// @module worker
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Shared;

public static class SlimTaskSchedulerExtensions
{
    public static IServiceCollection AddSlimTaskScheduler(this IServiceCollection services)
    {
        return services.AddTransient<SlimTaskScheduler>();
    }
}

/// <summary>
/// Runs an action on a cron schedule (Cronos syntax, 5 fields: "*/5 * * * *").
/// Every run gets its own DI scope; a failed run is logged and the schedule continues.
/// </summary>
public sealed class SlimTaskScheduler(IServiceScopeFactory scopeFactory, ILogger<SlimTaskScheduler> logger)
{
    public async Task ExecutePeriodicallyAsync(Func<IServiceProvider, Task> action, string expression, CancellationToken stoppingToken = default)
    {
        var cron = CronExpression.Parse(expression);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DelayThenExecute(cron, action, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{location} error", nameof(ExecutePeriodicallyAsync));
            }
        }
    }

    private async Task DelayThenExecute(CronExpression cron, Func<IServiceProvider, Task> action, CancellationToken stoppingToken)
    {
        var now = DateTimeOffset.Now;
        var next = cron.GetNextOccurrence(now, TimeZoneInfo.Local)
            ?? throw new InvalidOperationException("Cron expression has no next occurrence.");
        var delay = next - now;

        // Task.Delay accepts at most int.MaxValue milliseconds (~24.8 days); longer waits are chunked.
        if (delay.TotalMilliseconds > int.MaxValue)
        {
            delay = TimeSpan.FromMilliseconds(int.MaxValue);

            logger.LogDebug("Delaying for max allowed time chunk - now: {now} next: {next} delay: {delay}", now, next, delay);
            await Task.Delay(delay, stoppingToken);

            return;
        }

        logger.LogDebug("Delaying for final chunk - now: {now} next: {next} delay: {delay}", now, next, delay);
        await Task.Delay(delay, stoppingToken);

        using var scope = scopeFactory.CreateScope();
        await action(scope.ServiceProvider);
    }
}
