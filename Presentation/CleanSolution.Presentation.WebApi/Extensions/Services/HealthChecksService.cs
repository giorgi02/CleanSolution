using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanSolution.Presentation.WebApi.Extensions.Services;
public static class HealthChecksService
{
    /// <summary>
    /// ჯანმრთელობის შემოწმება (მოიცავს middleware -საც)
    /// გამოძახება: https://localhost:44377/health/ready
    /// </summary>
    public static void AddConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var downstreamServiceUrl = configuration["DownstreamService:BaseUrl"];

        services.AddHealthChecks()
          .AddUrlGroup(
             new Uri($"{downstreamServiceUrl}"),
             name: "Downstream API Health Check",
             failureStatus: HealthStatus.Unhealthy,
             timeout: TimeSpan.FromSeconds(3),
             tags: new string[] { "services" })
          .AddSqlServer(
             connectionString,
             name: "Database",
             failureStatus: HealthStatus.Degraded,
             timeout: TimeSpan.FromSeconds(1),
             tags: new string[] { "services" });
    }
}