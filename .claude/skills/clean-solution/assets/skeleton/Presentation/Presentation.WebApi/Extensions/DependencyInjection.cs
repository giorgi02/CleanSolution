global using Core.Shared.Exceptions;
global using Core.Shared.Extensions;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Presentation.WebApi.Extensions.Attributes;
global using System.Text;
using Asp.Versioning;
using AspNetCoreRateLimit;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Presentation.WebApi.Extensions.Services;
using Serilog;

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

        builder.Services.AddHttpContextAccessor(); // for IHttpContextAccessor injection
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddOpenApiInfo();

        builder.Services.AddMemoryCache();

        builder.Services.AddConfigureRateLimiting(builder.Configuration);

        var serilogLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("Project", "[__SOLUTION_NAME__]")
            .CreateLogger();
        builder.Logging.AddSerilog(serilogLogger, dispose: true);

        builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetString("ConnectionStrings:DefaultConnection"), tags: ["database"]);
        // every external HTTP dependency gets a named check on the same key its HttpClient uses
        // (unnamed URL checks share one default name and collide at runtime):
        // .AddUrlGroup(new Uri(builder.Configuration.GetString("ExternalServices:HrPortal")), name: "hrportal", tags: ["service"]);

        builder.Services.AddCors(options =>
        {
            string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
            string[] headers = builder.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>() ?? [];
            options.AddDefaultPolicy(configure => configure.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(headers));
            // WithOrigins       - front-end origins allowed to call the API
            // AllowAnyHeader    - request headers allowed (mostly relevant for the preflight OPTIONS call)
            // WithExposedHeaders - response headers the browser may read
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader(); // the version is part of the route: api/v{version:apiVersion}/...
        })
        .AddMvc() // versioning for controllers
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddCompression();

        // @if jwt
        builder.Services.AddJwtAuthenticationConfigs(builder.Configuration);
        builder.Services.AddJwtAuthorizationConfigs();
        // @endif
        // @if !jwt
        // JWT is optional: to protect every endpoint, call
        // builder.Services.AddJwtAuthenticationConfigs(builder.Configuration) and builder.Services.AddJwtAuthorizationConfigs()
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        // @endif
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
            options.EnableForHttps = true; // compress HTTPS responses
            options.Providers.Add<GzipCompressionProvider>(); // Accept-Encoding: gzip
            options.Providers.Add<BrotliCompressionProvider>(); // Accept-Encoding: br
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
}
