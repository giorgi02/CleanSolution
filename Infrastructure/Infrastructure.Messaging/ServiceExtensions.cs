using Core.Application.Interfaces.Services;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Producers;
using Infrastructure.Messaging.RequestServices;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;
public static class ServiceExtensions
{
    public static void AddMessagingLayer(this IServiceCollection services)
    {
        services.AddHostedService<UpsertPositionConsumer>();

        services.AddSingleton<IMessagingService, MessagingServices>();

        services.AddScoped<ICheckPersonsService, CheckPersonsService>();
    }
}