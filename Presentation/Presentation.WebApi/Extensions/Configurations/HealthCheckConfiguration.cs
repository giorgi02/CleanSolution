﻿using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Presentation.WebApi.Extensions.Configurations;

public static class HealthCheckConfiguration
{
    /// <summary>
    /// ჯანმრთელობის შემოწმება (service)
    /// გამოძახება: https://localhost:44377/health
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
             tags: new string[] { "databases" });
    }

    /// <summary>
    /// ჯანმრთელობის შემოწმება (middleware)
    /// Adds a Health Check endpoint to the <see cref="IEndpointRouteBuilder"/> with the specified template.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add endpoint to.</param>
    /// <param name="pattern">URL ის ნიმუში რომელიც გვიჩვენებს ყველა მდგომარეობას</param>
    /// <param name="servicesPattern">URL ის ნიმუში რომელიც გვიჩვენებს სერვისების მდგომარეობას</param>
    /// <param name="databasesPattern">URL ის ნიმუში რომელიც გვიჩვენებს მონაცემთა ბაზების მდგომარეობას</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapCustomHealthCheck(
       this IEndpointRouteBuilder endpoints,
       string pattern = "/health",
       string servicesPattern = "/health/services",
       string databasesPattern = "/health/databases"
        )
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        endpoints.MapHealthChecks(servicesPattern, new HealthCheckOptions()
        {
            Predicate = (check) => check.Tags.Contains("services"),
            AllowCachingResponses = false,
            ResponseWriter = WriteResponse,
        });
        endpoints.MapHealthChecks(databasesPattern, new HealthCheckOptions()
        {
            Predicate = (check) => check.Tags.Contains("databases"),
            AllowCachingResponses = false,
            ResponseWriter = WriteResponse,
        });
        endpoints.MapHealthChecks(pattern, new HealthCheckOptions()
        {
            Predicate = (check) => true,
            AllowCachingResponses = false,
            ResponseWriter = WriteResponse,
        });

        return endpoints;
    }

    private static Task WriteResponse(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions
        {
            Indented = true
        };

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", result.Status.ToString());
            writer.WriteStartObject("results");
            foreach (var entry in result.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());

        return context.Response.WriteAsync(json);
    }
}
