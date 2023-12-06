using Core.Application.Commons;
using Core.Application.Interfaces.Services;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Producers;
using Infrastructure.Messaging.RequestServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyGetterServiceReference;

namespace Infrastructure.Messaging;
public static class DependencyInjection
{
    public static IServiceCollection AddMessagingLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("HrPortal", client =>
        {
            client.BaseAddress = new Uri(configuration.GetString("ExternalServices:HrPortal"));
        });

        services.AddHostedService<UpsertPositionConsumer>();
        services.AddSingleton<IMessagingService, MessagingServices>();

        services.AddWcfServiceScoped<IPropertyGetterService, PropertyGetterServiceClient>();

        services.AddSingleton<ICheckPersonsService, CheckPersonsService>();

        return services;
    }
}