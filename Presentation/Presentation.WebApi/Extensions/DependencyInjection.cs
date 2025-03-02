global using MediatR;
global using System.Text;
using Core.Application.Commons;
using Core.Application.Interfaces.Services;
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

        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IActiveUserService, ActiveUserService>();

        builder.Services.AddSwaggerServices();

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
        });
    }
}