using Confluent.Kafka;
using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Messaging.Producers;
internal class MessagingServices : IMessagingService
{
    private const string _topic = "employee_created";
    private readonly string _bootstrapServers;

    public MessagingServices(IConfiguration configuration)
    {
        _bootstrapServers = configuration["ApacheKafka:BootstrapServers"] ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task EmployeeCreated(Employee employee)
    {
        string message = JsonSerializer.Serialize(employee);
        await SendOrderRequest(_topic, message);
    }

    private async Task<bool> SendOrderRequest(string topic, string message)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            //ClientId = Dns.GetHostName()
        };


        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var result = await producer.ProduceAsync(topic, new Message<Null, string>
        {
            Value = message
        });

        return await Task.FromResult(true);
    }
}