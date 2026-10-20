# Integrations

## Contents
1. Common rules
2. Kafka producer
3. Kafka consumer
4. Typed HTTP client (external REST API)
5. WCF client
6. File storage (MinIO)
7. In-process background queue (Channel, optional Rx)
8. Cron job (Presentation.Worker)
9. MCP tools (Presentation.Mcp)
10. Adding a missing module to an existing solution

---

## 1. Common rules

- **Port in Core.Application, adapter in Infrastructure.** The use case depends on an interface
  (`Interfaces/Services/IMessagingService`, `IHrPortalServices`, `IDocumentService`); the Infrastructure class implements it and
  is `internal sealed`. External contracts (message JSON, REST/WCF DTOs) live in the Infrastructure project (`Models/`), never in
  Core.Domain; the port speaks in domain types or Application DTOs.
- **Config keys**: brokers under `ApacheKafka`, REST base URLs under `ExternalServices:<Name>`, storage under `MinioConfigs`,
  WCF under `ServiceReferenceConfiguration`. Add the key to `appsettings.Development.json` when you read it in code, secrets to
  user-secrets.
- **Registration**: adapters in the Infrastructure project's `Add{Layer}Layer` (every host calls it - see
  `architecture.md` §5); background loops separately - Kafka consumers in `AddMessagingConsumers()`, called by exactly one
  host; queue workers and cron jobs with `AddHostedService` in the host that runs them.
- **Every non-HTTP entry point validates** (`ValidateOrThrowAsync`) and **creates a DI scope per unit of work**
  (`IServiceScopeFactory.CreateScope()`), and one failing message/item is logged and skipped - it never stops the loop.
- Side effects that leave the process (publish, upload, external call) come after the data they describe is saved.

## 2. Kafka producer

Port (one method per business event):

```csharp
// Core.Application/Interfaces/Services/IMessagingService.cs
using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;

public interface IMessagingService
{
    Task EmployeeCreated(Employee employee, CancellationToken cancellationToken = default);
}
```

Message contract and adapter (one long-lived producer; building one per message is expensive):

```csharp
// Infrastructure.Messaging/Models/EmployeeCreatedMessage.cs
namespace Infrastructure.Messaging.Models;

internal sealed record EmployeeCreatedMessage(Guid Id, string PrivateNumber, string FirstName, string LastName, DateTime CreatedAt);
```

```csharp
// Infrastructure.Messaging/Producers/MessagingServices.cs
using Confluent.Kafka;
using Core.Application.Interfaces.Services;
using Core.Domain.Models;
using Core.Shared.Extensions;
using Infrastructure.Messaging.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Messaging.Producers;

internal sealed class MessagingServices : IMessagingService, IDisposable
{
    private const string EmployeeCreatedTopic = "employee_created";

    private readonly IProducer<string, string> _producer;

    public MessagingServices(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration.GetString("ApacheKafka:BootstrapServers"),
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task EmployeeCreated(Employee employee, CancellationToken cancellationToken = default) =>
        ProduceAsync(EmployeeCreatedTopic, employee.Id,
            new EmployeeCreatedMessage(employee.Id, employee.PrivateNumber, employee.FirstName, employee.LastName, employee.CreatedAt),
            cancellationToken);

    // key = entity id: all events of one entity land in one partition, in order
    private async Task ProduceAsync<TMessage>(string topic, Guid key, TMessage message, CancellationToken cancellationToken)
    {
        var value = JsonSerializer.Serialize(message, JsonSerializerOptions.Web);
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key.ToString(), Value = value }, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
```

```csharp
// AddMessagingLayer
services.AddSingleton<IMessagingService, MessagingServices>();
```

Config: `"ApacheKafka": { "BootstrapServers": "localhost:9092", "Group": "<app>-group" }`. Topic names are snake_case event
names in a `const`. Publishing is not transactional with the database (no outbox in this template): call it after
`SaveChanges`, and make consumers idempotent.

## 3. Kafka consumer

The consumer turns each message into a Notification request and runs its handler in a fresh scope:

```csharp
// Infrastructure.Messaging/Consumers/UpsertPositionConsumer.cs
using Confluent.Kafka;
using Core.Application.Commons;
using Core.Application.Interactors.Notifications;
using Core.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Messaging.Consumers;

internal sealed class UpsertPositionConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<UpsertPositionConsumer> logger)
    : BackgroundService
{
    private const string Topic = "position_upsert";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = configuration.GetString("ApacheKafka:Group"),
            BootstrapServers = configuration.GetString("ApacheKafka:BootstrapServers"),
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Consume blocks the thread; run it off the host's startup path
                var result = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);
                await HandleAsync(result.Message.Value, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // host is stopping
        }
        finally
        {
            consumer.Close(); // commits offsets and leaves the group cleanly
        }
    }

    private async Task HandleAsync(string value, CancellationToken stoppingToken)
    {
        try
        {
            var request = JsonSerializer.Deserialize<UpsertPositionNotification.Request>(value, JsonSerializerOptions.Web);
            if (request is null) return;

            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.ValidateOrThrowAsync(request, stoppingToken);

            var handler = scope.ServiceProvider.GetRequiredService<UpsertPositionNotification.Handler>();
            await handler.Handle(request, stoppingToken);

            logger.LogInformation("messaging type={@type}, body={@body}", nameof(UpsertPositionConsumer), request);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            // a bad message is logged and skipped; it must not kill the consumer
            logger.LogError(ex, "{location} failed to process message {@body}", nameof(UpsertPositionConsumer), value);
        }
    }
}
```

```csharp
// Infrastructure.Messaging/DependencyInjection.cs - not in AddMessagingLayer
public static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
{
    services.AddHostedService<UpsertPositionConsumer>();
    return services;
}

// Program.cs of the one host that consumes (WebApi by default, or Presentation.Worker)
builder.Services.AddMessagingConsumers();
```

The handler (`UpsertPositionNotification`) lives in `Core.Application/Interactors/Notifications` and is registered in
`AddHandlers()` under `// notifications`. It must be idempotent (Kafka delivers at least once).

## 4. Typed HTTP client (external REST API)

```csharp
// Core.Application/Interfaces/Services/IHrPortalServices.cs
using Core.Application.DTOs;

namespace Core.Application.Interfaces.Services;

public interface IHrPortalServices
{
    /// <returns>null when HR Portal does not know the person</returns>
    Task<HrEmployeeDto?> GetEmployee(string personalNumber, CancellationToken cancellationToken = default);
}
```

```csharp
// Infrastructure.Messaging/Helpers/HrServiceEndpoints.cs
namespace Infrastructure.Messaging.Helpers;

internal static class HrServiceEndpoints
{
    // relative paths: no leading '/', the base address ends with '/'
    public static string GetEmployee(string personalNumber) => $"api/employees/{Uri.EscapeDataString(personalNumber)}";
}
```

```csharp
// Infrastructure.Messaging/RequestServices/HrPortalServices.cs
using Core.Application.DTOs;
using Core.Application.Interfaces.Services;
using Core.Shared.Exceptions;
using Infrastructure.Messaging.Helpers;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Infrastructure.Messaging.RequestServices;

internal sealed class HrPortalServices(HttpClient httpClient, ILogger<HrPortalServices> logger) : IHrPortalServices
{
    public async Task<HrEmployeeDto?> GetEmployee(string personalNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<HrEmployeeDto>(HrServiceEndpoints.GetEmployee(personalNumber), cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "{location} HR Portal call failed", nameof(HrPortalServices));
            throw new ApiValidationException("HR Portal service is unavailable. Try again later.");
        }
    }
}
```

```csharp
// AddMessagingLayer
services.AddHttpClient<IHrPortalServices, HrPortalServices>(client =>
{
    client.BaseAddress = new Uri(configuration.GetString("ExternalServices:HrPortal"));
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<CorrelationDelegatingHandler>();
```

```csharp
// Presentation.WebApi AddStartup - same key as the client; every URL check needs its own name
// (unnamed AddUrlGroup checks share a default name and the second one fails at runtime)
builder.Services.AddHealthChecks()
    .AddSqlServer(...)
    .AddUrlGroup(new Uri(builder.Configuration.GetString("ExternalServices:HrPortal")), name: "hrportal", tags: ["service"]);
```

Config: `"ExternalServices": { "HrPortal": "https://hr-portal.example/" }` (trailing slash). The response DTO
(`HrEmployeeDto`) is whatever the use case needs; if the remote shape differs, deserialize into an internal model in
`Infrastructure.Messaging/Models` and map it. For POST use `httpClient.PostAsync<TRequest, TResponse>(url, body, cancellationToken: ct)`
from `Core.Shared.Extensions`. Need retries? Add `Microsoft.Extensions.Http.Resilience` and `.AddStandardResilienceHandler()`
to the registration - only when asked.

## 5. WCF client

1. Generate the proxy into `Infrastructure.Messaging/Connected Services/<Name>Reference/` (Visual Studio "Add Connected Service →
   WCF Web Service Reference", or `dotnet-svcutil <wsdl-url> --outputDir "Connected Services/<Name>Reference" --namespace "*,<Name>Reference"`).
   Never edit `Reference.cs` by hand.
2. Packages: `System.ServiceModel.Http` (and `.Primitives`) `10.0.652802`; `.NetTcp`/`.Duplex`/`.Security`/`.Federation` only
   if the binding needs them. If NuGet then reports NU1903 for `System.Security.Cryptography.Xml`, reference its latest patch
   version explicitly.
3. `Helpers/WcfServiceConfiguration.cs` builds clients from configuration:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ServiceModel;

namespace Infrastructure.Messaging.Helpers;

public static class WcfServiceConfiguration
{
    public static void AddWcfServiceScoped<TServiceType, TImplementation>(this IServiceCollection services) =>
        services.AddScoped(typeof(TServiceType), serviceProvider => ImplementationFactory<TImplementation>(serviceProvider)!);

    private static TImplementation ImplementationFactory<TImplementation>(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var config = configuration.GetSection(nameof(ServiceReferenceConfiguration)).Get<ServiceReferenceConfiguration>()
            ?? throw new InvalidOperationException("Configuration section 'ServiceReferenceConfiguration' is missing.");

        var type = typeof(TImplementation);

        var endpoint = config.EndpointConfigs.FirstOrDefault(x => x.Name == type.Name)
            ?? throw new InvalidOperationException($"No endpoint configuration named '{type.Name}'.");

        var binding = config.BasicHttpBindings.FirstOrDefault(x => x.Name == endpoint.BindingConfiguration)
            ?? throw new InvalidOperationException($"No binding configuration named '{endpoint.BindingConfiguration}'.");

        return (TImplementation)(Activator.CreateInstance(type, binding, new EndpointAddress(endpoint.Address))
            ?? throw new InvalidOperationException($"Could not create {type.Name}."));
    }

    internal sealed class ServiceReferenceConfiguration
    {
        public List<BasicHttpBinding> BasicHttpBindings { get; set; } = [];
        public List<EndpointConfig> EndpointConfigs { get; set; } = [];
    }

    internal sealed class EndpointConfig
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string BindingConfiguration { get; set; } = null!;
    }
}
```

4. Register the generated client **and** an adapter that implements an Application port (use cases never see generated types):

```csharp
services.AddWcfServiceScoped<IPropertyGetterService, PropertyGetterServiceClient>(); // generated contract + client
services.AddScoped<IPropertyServices, PropertyServices>();                          // adapter: RequestServices/PropertyServices.cs
```

5. Config (endpoint `Name` = generated client class name):

```json
"ServiceReferenceConfiguration": {
  "BasicHttpBindings": [
    { "Name": "httpBinding", "SendTimeout": "00:00:10", "ReceiveTimeout": "00:00:10", "MaxReceivedMessageSize": 2147483647 }
  ],
  "EndpointConfigs": [
    { "Name": "PropertyGetterServiceClient", "Address": "http://localhost:58424/PropertyGetterService.svc", "BindingConfiguration": "httpBinding" }
  ]
}
```

## 6. File storage (MinIO)

The Documents module provides `IDocumentService` (`SaveAsync(FileContent, ct)` → unique object name, `GetAsync(objectName, ct)` →
bytes + content type), backed by `IMinioClient` (registered by `Minio.AspNetCore`; resolving the concrete `MinioClient` fails)
and `MinioSettings` bound from `MinioConfigs`.

Upload: controller `IFormFile` → `ToFileContent()` → `request.SetPicture(...)` → handler `documents.SaveAsync(file, ct)` → store
the returned object name on the entity (`PictureName`) → save. Download: a query loads the entity, calls `GetAsync(entity.PictureName, ct)`
and returns a nested `Response(byte[] FileData, string ContentType, string FileName)`; the controller returns `File(...)`
(`web-api.md` §4).

```csharp
public abstract class GetEmployeePictureQuery
{
    public record struct Request(Guid EmployeeId) : IRequest<Response>;

    public sealed record class Response(byte[] FileData, string ContentType, string FileName);


    public sealed class Handler(IEmployeeRepository repository, IDocumentService documents) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = await repository.ReadAsync(request.EmployeeId, cancellationToken)
                ?? throw new EntityNotFoundException("Employee was not found.", nameof(request.EmployeeId));

            if (employee.PictureName is null)
                throw new EntityNotFoundException("Employee has no picture.", nameof(request.EmployeeId));

            var (data, contentType) = await documents.GetAsync(employee.PictureName, cancellationToken);
            return new Response(data, contentType, employee.PictureName);
        }
    }
}
```

Bucket names are lowercase (`<app>-docs`); the bucket must exist. Access/secret keys in user-secrets.

## 7. In-process background queue

For work that should not block the HTTP request but doesn't need to survive a restart. (Needs durability or other services?
Use Kafka instead.) Default: `System.Threading.Channels`.

```csharp
// Core.Application/DependencyInjection.cs - called from AddApplicationLayer, so every host can build the handlers
private static void AddBackgroundQueues(this IServiceCollection services)
{
    services.AddSingleton(_ => Channel.CreateUnbounded<QueueItemDto>(new UnboundedChannelOptions
    {
        SingleReader = true,
        AllowSynchronousContinuations = false,
    }));
    services.AddSingleton(sp => sp.GetRequiredService<Channel<QueueItemDto>>().Writer); // handlers write
    services.AddSingleton(sp => sp.GetRequiredService<Channel<QueueItemDto>>().Reader); // the worker reads
}
```

```csharp
// Presentation.WebApi/Extensions/DependencyInjection.cs (AddStartup) - only the host that processes the queue
builder.Services.AddHostedService<CheckoutProcessWorker>();
```

The queue lives in one process: only the host that runs the worker may expose commands that write to it (an MCP tool or a cron
job enqueuing into its own process's channel would never be processed).

Command (returns 202 with a tracking id; see `web-api.md` §5):

```csharp
public abstract class CheckoutCommand
{
    public record struct Request(int ItemsCount, decimal Amount) : IRequest<Response>;

    public sealed record class Response
    {
        public Guid OrderId { get; init; }
        public required string Message { get; init; }
    }


    public sealed class Handler(ChannelWriter<QueueItemDto> queue) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();
            await queue.WriteAsync(new QueueItemDto { OrderId = orderId, Text = $"Order {request.ItemsCount} {request.Amount}" }, cancellationToken);

            return new Response { OrderId = orderId, Message = "Your order is being processed. You will receive the details by email." };
        }
    }
}
```

Worker:

```csharp
// Presentation.WebApi/Workers/CheckoutProcessWorker.cs
using Core.Application.DTOs;
using Core.Application.Interactors.Notifications;
using System.Threading.Channels;

namespace Presentation.WebApi.Workers;

public sealed class CheckoutProcessWorker(IServiceScopeFactory scopeFactory, ChannelReader<QueueItemDto> reader, ILogger<CheckoutProcessWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<CheckoutProcessNotification.Handler>();
                await handler.Handle(new CheckoutProcessNotification.Request(item), stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "{location} failed for {@item}", nameof(CheckoutProcessWorker), item);
            }
        }
    }
}
```

Unbounded channels grow without limit if the worker falls behind; for bursty producers use
`Channel.CreateBounded<T>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait })`.

Rx alternative (`IObserver<T>`/`IObservable<T>`) only when several independent subscribers must see every item. Then: reference
`System.Reactive` explicitly; use `Subject<T>` (a `ReplaySubject<T>` replays and keeps **every** item forever - a memory leak);
never subscribe with an `async` lambda (`Subscribe(async x => ...)` is `async void`: one exception crashes the process) - use
`stream.Select(item => Observable.FromAsync(ct => ProcessAsync(item, ct))).Concat().Subscribe(...)` with error handling inside
`ProcessAsync`.

## 8. Cron job (Presentation.Worker)

The job's logic is a Notification use case; the worker only schedules it.

```csharp
// Presentation.Worker/Worker.cs
using Core.Application.Commons;
using Core.Application.Interactors.Notifications;
using Core.Shared;
using Core.Shared.Extensions;

namespace Presentation.Worker;

public sealed class Worker(SlimTaskScheduler scheduler, IConfiguration configuration, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await scheduler.ExecutePeriodicallyAsync(async services =>
        {
            var request = new SyncPositionsNotification.Request();
            await services.ValidateOrThrowAsync(request, stoppingToken);

            var handler = services.GetRequiredService<SyncPositionsNotification.Handler>();
            await handler.Handle(request, stoppingToken);

            logger.LogInformation("{job} finished at {time}", nameof(SyncPositionsNotification), DateTimeOffset.Now);
        }, configuration.GetString("CronExpression"), stoppingToken);
    }
}
```

- `SlimTaskScheduler` (Core.Shared, Cronos) gives each run its own scope and logs a failed run without stopping the schedule.
- Cron syntax is 5 fields (`"*/5 * * * *"` = every 5 minutes, local time zone). One schedule per `BackgroundService`; for
  several jobs use one class per job with its own key (`"Jobs:SyncPositions:CronExpression"`).
- `Program.cs` registers `IActiveUserService` as `SystemActiveUserService`, then **all** layers the use cases depend on
  (`AddApplicationLayer`, `AddPersistenceLayer`, `AddDocumentsLayer`, `AddMessagingLayer` - whichever exist; not the consumers),
  `AddSlimTaskScheduler()` and `AddHostedService<Worker>()`. Don't register handlers one by one in the worker host -
  `AddApplicationLayer` already does, and Development's DI validation needs every handler's dependencies present.
- If the same job could run on several instances at once, it must be idempotent or take a lock (DB row / distributed lock).

## 9. MCP tools (Presentation.Mcp)

The MCP server (`ModelContextProtocol.AspNetCore`, stateless HTTP transport at `/mcp`) exposes use cases to AI agents. A tool
class is the MCP equivalent of a controller: thin, calls handlers, no business logic.

```csharp
// Presentation.Mcp/Tools/EmployeeTools.cs
using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Application.Interactors.Queries;
using Core.Shared;
using Core.Shared.Exceptions;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Presentation.Mcp.Tools;

[McpServerToolType]
public sealed class EmployeeTools(IServiceProvider services, GetEmployeesQuery.Handler getEmployees, GetEmployeeQuery.Handler getEmployee)
{
    [McpServerTool(Name = "search_employees", ReadOnly = true), Description("Returns one page of employees, optionally filtered by first name.")]
    public async Task<Pagination<GetEmployeeDto>> SearchEmployees(
        [Description("Exact first name to filter by; omit for all")] string? firstName = null,
        [Description("1-based page number")] int pageIndex = 1,
        [Description("Page size between 1 and 100")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetEmployeesQuery.Request { FirstName = firstName, PageIndex = pageIndex, PageSize = pageSize };
        return await RunAsync(request, () => getEmployees.Handle(request, cancellationToken), cancellationToken);
    }

    [McpServerTool(Name = "get_employee", ReadOnly = true), Description("Returns one employee by id.")]
    public async Task<GetEmployeeDto> GetEmployee([Description("Employee id (GUID)")] Guid employeeId, CancellationToken cancellationToken = default)
    {
        var request = new GetEmployeeQuery.Request(employeeId);
        return await RunAsync(request, () => getEmployee.Handle(request, cancellationToken), cancellationToken);
    }

    // validation + turning expected errors into messages the model can read (other exceptions become a generic error)
    private async Task<TResult> RunAsync<TRequest, TResult>(TRequest request, Func<Task<TResult>> handle, CancellationToken cancellationToken)
    {
        try
        {
            await services.ValidateOrThrowAsync(request, cancellationToken);
            return await handle();
        }
        catch (ApiValidationException ex)
        {
            throw new McpException(string.Join(" ", ex.Messages.SelectMany(x => x.Value)));
        }
    }
}
```

- `WithToolsFromAssembly()` discovers every `[McpServerToolType]` class; tool classes get handlers through the constructor
  (stateless mode uses the HTTP request scope).
- Tool and parameter `Description`s are the model's only documentation - write them precisely, in English; snake_case `Name`.
- Mark read-only tools `ReadOnly = true`; mark destructive ones `Destructive = true`.
- `Program.cs` registers `SystemActiveUserService` and every layer (`AddApplicationLayer`, `AddPersistenceLayer`,
  `AddDocumentsLayer`, `AddMessagingLayer` - whichever exist; not the consumers) before `AddMcpServer()`.

## 10. Adding a missing module to an existing solution

If the solution has no Infrastructure.Messaging / Infrastructure.Documents / Presentation.Worker / Presentation.Mcp yet:
1. Copy the project folder from `assets/skeleton/` into the same place in the solution (replace `__SOLUTION_NAME__` and
   `__USER_SECRETS_ID__` - a new GUID - and drop `// @if` / `// @endif` / `// @module` marker lines).
2. `dotnet sln <name>.slnx add <path-to-csproj> --solution-folder <Infrastructure|Presentation>` (or edit the `.slnx`).
3. Reference it from the host(s) and call its `Add{Layer}Layer` in `Program.cs`; add its config section.
4. `dotnet build`.
