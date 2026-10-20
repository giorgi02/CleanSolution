global using Core.Shared.Exceptions;
global using FluentValidation;
using Core.Application.Commons;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddHandlers();
        services.RegisterMapsterConfiguration();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    /// <summary>
    /// Every use-case handler is registered here, explicitly and as transient.
    /// A handler missing from this list fails at runtime when it is requested via [FromServices].
    /// </summary>
    private static void AddHandlers(this IServiceCollection services)
    {
        // commands

        // queries

        // notifications
    }
}
