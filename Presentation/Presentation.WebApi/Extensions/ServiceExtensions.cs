global using System.Reflection;
global using System.Text;
global using System.Text.Json;

using AspNetCoreRateLimit;
using Core.Application.Interfaces.Services;
using FluentValidation.AspNetCore;
using Presentation.WebApi.Extensions.Attributes;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Services;

namespace Presentation.WebApi.Extensions;
public static class ServiceExtensions
{
    public static void AddThisLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add(typeof(ActionLoggingAttribute)));

        services.AddFluentValidationAutoValidation();

        services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        services.AddScoped<IActiveUserService, ActiveUserService>();

        services.AddSwaggerServices();
        services.AddConfigureHealthChecks(configuration);

        services.AddLocalizeConfiguration();

        services.AddMemoryCache();

        services.AddConfigureRateLimiting(configuration);

        services.AddCors(options =>
        {
            string[] headers = configuration.GetSection("ExposedHeaders").Get<string[]>();
            options.AddDefaultPolicy(configure
                => configure.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // AllowAnyHeader - დაშვება Request-ის Header-ებზე, ძირითადად გამოიყენება preflight ის დროს [OPTIONS] მეთოდისთვის
            // WithExposedHeaders - დაშვება Response-ის სპეციფიურ Header-ებზე
        });
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