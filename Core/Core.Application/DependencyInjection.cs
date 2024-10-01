global using FluentValidation;
global using MediatR;
global using System.Net;

using Core.Application.Behaviors;
using Core.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicatonLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.NotificationPublisher = new TaskWhenAllPublisher(); // default: new ForeachAwaitPublisher();
        });
        services.RegisterMapsterConfiguration();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
