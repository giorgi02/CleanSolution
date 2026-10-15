global using Core.Shared.Exceptions;
global using FluentValidation;
using Core.Application.Commons;
using Core.Application.Interactors.Commands;
using Core.Application.Interactors.Notifications;
using Core.Application.Interactors.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicatonLayer(this IServiceCollection services, IConfiguration _)
    {
        services.AddHandlers();
        services.RegisterMapsterConfiguration();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    private static void AddHandlers(this IServiceCollection services)
    {
        // commands
        services.AddTransient<CheckoutCommand1.Handler>();
        services.AddTransient<CheckoutCommand2.Handler>();
        services.AddTransient<CreateEmployeeCommand.Handler>();
        services.AddTransient<DeleteEmployeeCommand.Handler>();
        services.AddTransient<UpdateEmployeeCommand.Handler>();

        // queries
        services.AddTransient<GetEmployeeQuery.Handler>();
        services.AddTransient<GetEmployeesQuery.Handler>();
        services.AddTransient<GetFakeEmployeesQuery.Handler>();
        services.AddTransient<GetMaxMinBySalaryQuery.Handler>();
        services.AddTransient<GetPositionQuery.Handler>();
        services.AddTransient<GetPositionsQuery.Handler>();

        // notifications
        services.AddTransient<CheckoutProcessCommand.Handler>();
        services.AddTransient<UpsertPositionNotification.Handler>();
    }
}
