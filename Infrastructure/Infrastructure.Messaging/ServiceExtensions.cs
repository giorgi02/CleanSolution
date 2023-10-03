using Core.Application.Interfaces.Services;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Producers;
using Infrastructure.Messaging.RequestServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyGetterServiceReference;

namespace Infrastructure.Messaging;
public static class ServiceExtensions
{
    public static void AddMessagingLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<UpsertPositionConsumer>();

        services.AddSingleton<IMessagingService, MessagingServices>();

        services.AddWcfServiceScoped<IPropertyGetterService, PropertyGetterServiceClient>();

        services.AddScoped<ICheckPersonsService, CheckPersonsService>();
    }
}