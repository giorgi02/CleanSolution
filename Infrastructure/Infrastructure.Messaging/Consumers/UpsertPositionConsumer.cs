using Confluent.Kafka;
using Core.Application.Interactors.Notifications;
using Core.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Messaging.Consumers;

internal class UpsertPositionConsumer : BackgroundService
{
    private const string Topic = "position_upsert";
    private const long ErrorLogIntervalMs = 30_000;
    private readonly Dictionary<string, long> _lastErrorLogs = [];
    private readonly string _groupId;
    private readonly string _bootstrapServers;

    private readonly IServiceProvider _services;
    private readonly ILogger<UpsertPositionConsumer> _logger;

    public UpsertPositionConsumer(IServiceProvider services, IConfiguration configuration, ILogger<UpsertPositionConsumer> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
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
            EnableAutoOffsetStore = true,
            ReconnectBackoffMs = 5000,
            ReconnectBackoffMaxMs = 30000
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config)
            .SetLogHandler((_, log) => LogKafka(log))
            .Build();
        try
        {
            consumer.Subscribe(Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);
                var request = JsonSerializer.Deserialize<UpsertPositionNotification.Request>(consumeResult.Message.Value);

                if (request != null)
                {
                    using var scope = _services.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<UpsertPositionNotification.Handler>();
                    await handler.Handle(request, stoppingToken);
                }

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

    // librdkafka repeats the same connection failure several times per second while a broker is unreachable
    private void LogKafka(LogMessage log)
    {
        var level = ToLogLevel(log.Level);

        if (level >= LogLevel.Error && !IsErrorLogDue(log.Facility))
            return;

        _logger.Log(level, "{location} {facility} {message}", nameof(UpsertPositionConsumer), log.Facility, log.Message);
    }

    private bool IsErrorLogDue(string facility)
    {
        lock (_lastErrorLogs)
        {
            var now = Environment.TickCount64;

            if (_lastErrorLogs.TryGetValue(facility, out var last) && now - last < ErrorLogIntervalMs)
                return false;

            _lastErrorLogs[facility] = now;
            return true;
        }
    }

    private static LogLevel ToLogLevel(SyslogLevel level) => level switch
    {
        SyslogLevel.Emergency or SyslogLevel.Alert or SyslogLevel.Critical => LogLevel.Critical,
        SyslogLevel.Error => LogLevel.Error,
        SyslogLevel.Warning => LogLevel.Warning,
        SyslogLevel.Notice or SyslogLevel.Info => LogLevel.Information,
        _ => LogLevel.Debug
    };
}