using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolution.Infrastructure.EventSourcing;
public static class ServiceExtensions
{
    public static void AddEventSourcingLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var eventStoreConnection = EventStoreConnection.Create(
            connectionString: configuration.GetValue<string>("EventStore:ConnectionString"),
            builder: ConnectionSettings.Create().KeepReconnecting(),
            connectionName: configuration.GetValue<string>("EventStore:ConnectionName"));

        eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();

        services.AddSingleton(eventStoreConnection);
        //services.AddTransient<AggregateRepository>();
    }
}