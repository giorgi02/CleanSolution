using Core.Shared.Extensions;
using Infrastructure.Messaging.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

public static class DependencyInjection
{
    /// <summary>
    /// Adapters for the Application ports (HTTP clients, Kafka producers, WCF clients).
    /// Every host whose use cases need these ports calls it.
    /// </summary>
    public static IServiceCollection AddMessagingLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<CorrelationDelegatingHandler>();

        // typed HTTP clients (base address from "ExternalServices:<Name>")
        // services.AddHttpClient<IHrPortalServices, HrPortalServices>(client =>
        //     client.BaseAddress = new Uri(configuration.GetString("ExternalServices:HrPortal")))
        //     .AddHttpMessageHandler<CorrelationDelegatingHandler>();

        // kafka producers (singleton)
        // services.AddSingleton<IMessagingService, MessagingServices>();

        return services;
    }

    /// <summary>
    /// Kafka consumers (hosted services). Called by exactly one host - the one that should consume -
    /// so the same messages are not processed by every process that composes the messaging layer.
    /// </summary>
    public static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
    {
        // services.AddHostedService<UpsertPositionConsumer>();

        return services;
    }
}
