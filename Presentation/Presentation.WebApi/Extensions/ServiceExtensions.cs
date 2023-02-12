global using System.Reflection;
global using System.Text;
global using System.Text.Json;

using AspNetCoreRateLimit;
using Core.Application.Interfaces.Services;
using FluentValidation.AspNetCore;
using Presentation.WebApi.Extensions.Attributes;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Services;
using Serilog;

namespace Presentation.WebApi.Extensions;
public static class ServiceExtensions
{
    public static void AddThisLayer(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options => options.Filters.Add(typeof(ActionLoggingAttribute)));

        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddSwaggerServices();
        builder.Services.AddConfigureHealthChecks(builder.Configuration);

        builder.Services.AddLocalizeConfiguration();

        builder.Services.AddMemoryCache();

        builder.Services.AddConfigureRateLimiting(builder.Configuration);

        builder.Host.AddLoggerLayer(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            string[] headers = builder.Configuration.GetSection("ExposedHeaders").Get<string[]>() ?? throw new ArgumentNullException("ExposedHeaders");
            options.AddDefaultPolicy(configure
                => configure.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // AllowAnyHeader - დაშვება Request-ის Header-ებზე, ძირითადად გამოიყენება preflight ის დროს [OPTIONS] მეთოდისთვის
            // WithExposedHeaders - დაშვება Response-ის სპეციფიურ Header-ებზე
        });
    }

    // დაილოგება Seq ლოგების მენეჯერში
    public static void AddLoggerLayer(this IHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((context, config) => config
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("Project", "[CleanSolution]")
            .WriteTo.Seq("http://localhost:5341", period: new TimeSpan(0, 0, 10)));
    }


    private static void AddConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        //load general configuration from appsettings.json
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