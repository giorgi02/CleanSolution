global using Serilog;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;

using AspNetCoreRateLimit;
using CleanSolution.Core.Application.Interfaces.Services;
using CleanSolution.Presentation.WebApi.Extensions.Configurations;
using CleanSolution.Presentation.WebApi.Extensions.Services;
using FluentValidation.AspNetCore;


namespace CleanSolution.Presentation.WebApi.Extensions;
public static class ServiceExtensions
{
    public static void AddThisLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddFluentValidation();

        services.AddHttpContextAccessor(); // IHttpContextAccessor -ის ინექციისთვის
        services.AddScoped<IActiveUserService, ActiveUserService>();

        services.AddConfigureCors();
        services.AddSwaggerServices();
        services.AddConfigureHealthChecks(configuration);

        services.AddLocalizeConfiguration(configuration);

        services.AddMemoryCache();

        services.AddConfigureRateLimiting(configuration);
    }


    private static void AddConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "CorsPolicy", builder => builder
                .AllowAnyOrigin() // დაშვება ეძლევა მოთხოვნას ნებისმიერი წყაროდან
                .AllowAnyMethod() // დაშვებას იძლევა HTTP ყველა მეთოდზე
                .AllowAnyHeader()
                .WithExposedHeaders("AccessToken",
                "PageIndex", "PageSize",
                "TotalPages", "TotalCount",
                "HasPreviousPage", "HasNextPage"));
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
    //            // Add XML Content Negotiation
    //            options.RespectBrowserAcceptHeader = true;
    //        options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
    //        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
    //    });
    //}
}