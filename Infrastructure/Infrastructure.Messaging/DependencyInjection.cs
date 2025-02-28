using Core.Application.Commons;
using Core.Application.Interfaces.Services;
using Infrastructure.Messaging.Producers;
using Infrastructure.Messaging.RequestServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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


        services.AddSingleton<IMessagingService, MessagingServices>();

        return services;
    }
}