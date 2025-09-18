using Core.Application.Interfaces.Services;
using Core.Shared;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Helpers;
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
        services.AddTransient<CorrelationDelegatingHandler>();

        services.AddHttpClient<IHrPortalServices, HrPortalServices>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetString("ServiceUrls:HrPortal"));
        }).AddHttpMessageHandler<CorrelationDelegatingHandler>();


        services.AddHostedService<UpsertPositionConsumer>();
        services.AddSingleton<IMessagingService, MessagingServices>();


        services.AddWcfServiceScoped<IPropertyGetterService, PropertyGetterServiceClient>();

        return services;
    }
}