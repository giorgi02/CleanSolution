global using FluentValidation;
global using MediatR;
global using System.Net;

using Core.Application.Behaviors;
using Core.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application;
public static class ServiceExtensions
{
    public static void AddApplicatonLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.RegisterMapsterConfiguration();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}
