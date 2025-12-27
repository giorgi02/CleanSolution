using Confluent.Kafka;
using Core.Application.Interactors.Notifications;
using Core.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Messaging.Consumers;

internal class UpsertPositionConsumer : BackgroundService
{
    private const string Topic = "position_upsert";
    private readonly string _groupId;
    private readonly string _bootstrapServers;

    private readonly IMediator _mediator;
    private readonly ILogger<UpsertPositionConsumer> _logger;

    public UpsertPositionConsumer(IMediator mediator, IConfiguration configuration, ILogger<UpsertPositionConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _groupId = configuration.GetString("ApacheKafka:Group");
        _bootstrapServers = configuration.GetString("ApacheKafka:BootstrapServers");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = _groupId,
            BootstrapServers = _bootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        try
        {
            consumer.Subscribe(Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);
                var request = JsonSerializer.Deserialize<UpsertPositionNotification.Request>(consumeResult.Message.Value);

                if (request != null) await _mediator.Publish(request, stoppingToken);

                _logger.LogInformation("messaging type={@type}, body={@body}", nameof(UpsertPositionConsumer), request);
            }
        }
        catch (OperationCanceledException ex)
        {
            consumer.Close();

            _logger.LogWarning(ex, nameof(UpsertPositionConsumer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(UpsertPositionConsumer));
        }
    }
}