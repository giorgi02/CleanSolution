global using Core.Application.DTOs;
global using Core.Application.Interactors.Commands;
global using Core.Application.Interactors.Queries;
global using MediatR;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Presentation.WebApi.Extensions.Attributes;
global using System.Text;
using Asp.Versioning;
using AspNetCoreRateLimit;
using Core.Application.Interfaces.Services;
using Core.Shared;
using Microsoft.AspNetCore.ResponseCompression;
using Presentation.WebApi.Extensions.Services;
using Presentation.WebApi.Workers;
using Serilog;
using System.Reactive.Subjects;
using System.Threading.Channels;

namespace Presentation.WebApi.Extensions;

public static class DependencyInjection
{
    public static void AddStartup(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ActionLoggingAttribute>();
            options.Filters.Add<FluentValidationFilter>();
        });

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddOpenApiInfo();

        builder.Services.AddLocalizeConfiguration();

        builder.Services.AddMemoryCache();

        builder.Services.AddConfigureRateLimiting(builder.Configuration);

        var serilogLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("Project", "[CleanSolution]")
            .CreateLogger();
        builder.Logging.AddSerilog(serilogLogger, dispose: true);

        builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, tags: ["database"])
            .AddUrlGroup(new Uri(builder.Configuration.GetString("ExternalServices:HrPortal")), tags: ["service"]);

        builder.Services.AddCors(options =>
        {
            string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
            string[] headers = builder.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>() ?? [];
            options.AddDefaultPolicy(configure => configure.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // AllowAnyOrigin - ფრონტის ფრეიმვორკებს ყველა მისამართიდან შეუძლიათ გამოძახება
            // AllowAnyHeader - დაშვება Request-ის Header-ებზე, ძირითადად გამოიყენება preflight ის დროს [OPTIONS] მეთოდისთვის
            // WithExposedHeaders - დაშვება Response-ის სპეციფიურ Header-ებზე
            // AllowCredentials - 
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        builder.Services.AddObservable();

        builder.Services.AddCompression();
    }

    private static void AddConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        // inject counter and rules
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

        // configuration (resolvers, counter key builders)
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        // inject counter and rules stores
        services.AddInMemoryRateLimiting();
    }

    private static void AddCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true; // Compress HTTPS responses
            options.Providers.Add<GzipCompressionProvider>(); // usage: gzip
            options.Providers.Add<BrotliCompressionProvider>(); // usage: rb
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });
    }

    private static void AddObservable(this IServiceCollection services)
    {
        services.AddSingleton<ReplaySubject<QueueItemDto>>();
        services.AddSingleton<IObservable<QueueItemDto>>(x => x.GetRequiredService<ReplaySubject<QueueItemDto>>());
        services.AddSingleton<IObserver<QueueItemDto>>(x => x.GetRequiredService<ReplaySubject<QueueItemDto>>());

        services.AddHostedService<LongRunningTask1Worker>();
        services.AddHostedService<LongRunningTask2Worker>();

        services.AddSingleton(_ => Channel.CreateUnbounded<QueueItemDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false,
        }));
    }

    //private static void AddConfigureXml(this IServiceCollection services)
    //{
    //    services.AddMvc(options =>
    //    {
    //        // Add XML Content Negotiation
    //        options.RespectBrowserAcceptHeader = true;
    //        options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
    //        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
    //    });
    //}
}