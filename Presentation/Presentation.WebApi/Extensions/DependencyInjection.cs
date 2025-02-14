global using MediatR;
global using System.Text;
using Asp.Versioning;
using AspNetCoreRateLimit;
using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Application.Interfaces.Services;
using FluentValidation.AspNetCore;
using Presentation.WebApi.Extensions.Attributes;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Services;
using Presentation.WebApi.Workers;
using Serilog;
using System.Reactive.Subjects;
using System.Threading.Channels;

namespace Presentation.WebApi.Extensions;
public static class DependencyInjection
{
    public static void AddThisLayer(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(typeof(ActionLoggingAttribute));
        });

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddSwaggerServices();

        builder.Services.AddLocalizeConfiguration();

        builder.Services.AddMemoryCache();

        builder.Services.AddConfigureRateLimiting(builder.Configuration);

        builder.Host.UseSerilog((_, config) => config
           .ReadFrom.Configuration(builder.Configuration)
           .Enrich.WithProperty("Project", "[CleanSolution]"));

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

        //Observable Registartions
        builder.Services.AddSingleton<ReplaySubject<QueueItemDto>>();
        builder.Services.AddSingleton<IObservable<QueueItemDto>>(x => x.GetRequiredService<ReplaySubject<QueueItemDto>>());
        builder.Services.AddSingleton<IObserver<QueueItemDto>>(x => x.GetRequiredService<ReplaySubject<QueueItemDto>>());

        builder.Services.AddHostedService<LongRunningTask1Worker>();
        builder.Services.AddHostedService<LongRunningTask2Worker>();

        builder.Services.AddSingleton(_ => Channel.CreateUnbounded<QueueItemDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false,
        }));
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