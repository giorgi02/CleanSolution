global using System.Reflection;
global using System.Text;
global using System.Text.Json;
using Asp.Versioning;
using AspNetCoreRateLimit;
using Core.Application.Interfaces.Services;
using FluentValidation.AspNetCore;
using Presentation.WebApi.Extensions.Attributes;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Services;
using Serilog;

namespace Presentation.WebApi.Extensions;
public static class DependencyInjection
{
    public static void AddThisLayer(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(typeof(ActionLoggingAttribute));
        });

        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddSwaggerServices();
        builder.Services.AddConfigureHealthChecks(builder.Configuration);

        builder.Services.AddLocalizeConfiguration();

        builder.Services.AddMemoryCache();

        builder.Services.AddConfigureRateLimiting(builder.Configuration);

        builder.Host.UseSerilog((_, config) => config
           .ReadFrom.Configuration(builder.Configuration)
           .Enrich.WithProperty("Project", "[CleanSolution]"));

        builder.Services.AddCors(options =>
        {
            string[] headers = builder.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>() ?? throw new ArgumentNullException(nameof(builder.Configuration));
            string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? throw new ArgumentNullException(nameof(builder.Configuration));
            options.AddDefaultPolicy(configure
                => configure.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // => configure.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // AllowAnyHeader - დაშვება Request-ის Header-ებზე, ძირითადად გამოიყენება preflight ის დროს [OPTIONS] მეთოდისთვის
            // WithExposedHeaders - დაშვება Response-ის სპეციფიურ Header-ებზე
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
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