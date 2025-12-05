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

public class SlimTaskScheduler(IServiceProvider services, ILogger<SlimTaskScheduler> logger)
{
    private CronExpression cron = null!;

    public async Task ExecutePeriodicallyAsync(Func<IServiceProvider, Task> action, string expression, CancellationToken stoppingToken = default)
    {
        cron = CronExpression.Parse(expression);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DelayThenExecute(action, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{location} error", nameof(ExecutePeriodicallyAsync));
                throw;
            }
        }
    }

    private async Task DelayThenExecute(Func<IServiceProvider, Task> action, CancellationToken stoppingToken)
    {
        var now = DateTimeOffset.Now;
        var next = cron.GetNextOccurrence(now, TimeZoneInfo.Local)!.Value;
        var delay = next - now;

        if (delay.TotalMilliseconds > int.MaxValue)
        {
            delay = TimeSpan.FromMilliseconds(int.MaxValue);

            logger.LogDebug("Delaying for max allowed time chunk - now: {now} next: {next} delay: {delay}", now, next, delay);
            await Task.Delay(delay, stoppingToken);

            return;
        }

        logger.LogDebug("Delaying for final chunk - now: {now} next: {next} delay: {delay}", now, next, delay);
        await Task.Delay(delay, stoppingToken);

        using var scope = services.CreateScope();
        await action(scope.ServiceProvider);
    }
}