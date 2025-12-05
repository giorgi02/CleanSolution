using Confluent.Kafka;
using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Core.Shared;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Messaging.Producers;

internal class MessagingServices : IMessagingService
{
    private const string Topic = "employee_created";
    private readonly string _bootstrapServers;

    public MessagingServices(IConfiguration configuration) =>
        _bootstrapServers = configuration.GetString("ApacheKafka:BootstrapServers");


    public async Task EmployeeCreated(Employee employee)
    {
        string message = JsonSerializer.Serialize(employee);
        await SendOrderRequest(Topic, message);
    }

    private async Task SendOrderRequest(string topic, string message)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            //ClientId = Dns.GetHostName()
        };


        using var producer = new ProducerBuilder<Null, string>(config).Build();

        await producer.ProduceAsync(topic, new Message<Null, string>
        {
            Value = message
        });
    }
}