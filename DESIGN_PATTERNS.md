# პროექტში გამოყენებული დიზაინ პატერნები

ეს `CleanSolution` არის .NET 9/10 პროექტი რომელიც აშენებულია **Clean Architecture** პრინციპებზე და უხვად იყენებს მრავალ კლასიკურ თუ თანამედროვე დიზაინ პატერნს. ქვემოთ ჩამოვწერ ყველაფერს რასაც ვხედავ.

---

## 1. არქიტექტურული პატერნები (Architectural)

### 1.1 Clean Architecture (Onion / Hexagonal)
პროექტი დაყოფილია სამ ფენად, dependency-ები მიდის შიგნით ცენტრისკენ:
- **Core** (Domain, Application, Shared) — ბიზნეს ლოგიკა, არ არის დამოკიდებული გარე სამყაროზე
- **Infrastructure** (Persistence, Messaging, Documents) — გარე სამყაროსთან კავშირი
- **Presentation** (WebApi, Worker, Mcp) — შესვლის წერტილები

ეს ნათლად ჩანს [Program.cs](Presentation/Presentation.WebApi/Program.cs#L17-L21) — სად ცალ-ცალკე ემატება ფენები.

### 1.2 CQRS (Command Query Responsibility Segregation)
ნათლად გამოყოფილია:
- **Commands**: [CreateEmployeeCommand](Core/Core.Application/Interactors/Commands/CreateEmployeeCommand.cs), [UpdateEmployeeCommand](Core/Core.Application/Interactors/Commands/UpdateEmployeeCommand.cs), [DeleteEmployeeCommand](Core/Core.Application/Interactors/Commands/DeleteEmployeeCommand.cs)
- **Queries**: [GetEmployeeQuery](Core/Core.Application/Interactors/Queries/GetEmployeeQuery.cs), [GetEmployeesQuery](Core/Core.Application/Interactors/Queries/GetEmployeesQuery.cs), [GetMaxMinBySalaryQuery](Core/Core.Application/Interactors/Queries/GetMaxMinBySalaryQuery.cs)

### 1.3 Event Sourcing
[Aggregate](Core/Core.Domain/Basics/Aggregate.cs) კლასი იყენებს `Apply()`/`When()`/`Load()` მექანიზმებს, ხოლო [LogEventRepository](Infrastructure/Infrastructure.Persistence/Implementations/LogEventRepository.cs) ინახავს და აღადგენს state-ს მოვლენების მიხედვით (versioning + time-travel).

### 1.4 Domain-Driven Design (DDD)
- [BaseEntity](Core/Core.Domain/Basics/BaseEntity.cs), [AuditableEntity](Core/Core.Domain/Basics/AuditableEntity.cs), [Aggregate](Core/Core.Domain/Basics/Aggregate.cs) — ნამდვილი DDD building blocks
- [Address](Core/Core.Domain/Models/Address.cs) — Value Object (immutable `record`)
- Owned Types კონფიგურაცია [EmployeeConfiguration.cs:30-31](Infrastructure/Infrastructure.Persistence/Configurations/EmployeeConfiguration.cs#L30-L31)

### 1.5 Layered / N-Tier Architecture
თითო ფენას აქვს თავისი `DependencyInjection.cs` (Composition Root პერ-ფენაში).

---

## 2. შემოქმედებითი პატერნები (Creational)

### 2.1 Factory Method
- [EntityNotFoundException.Create()](Core/Core.Shared/Exceptions/EntityNotFoundException.cs#L13)
- [OperationForbiddenException.Create()](Core/Core.Shared/Exceptions/OperationForbiddenException.cs#L13)
- [Pagination<T>.Create()](Core/Core.Shared/Pagination.cs#L9) — სტატიკური factory method

### 2.2 Builder
- `ProducerBuilder<Null, string>` / `ConsumerBuilder<Ignore, string>` Kafka-ში ([MessagingServices.cs](Infrastructure/Infrastructure.Messaging/Producers/MessagingServices.cs), [UpsertPositionConsumer.cs](Infrastructure/Infrastructure.Messaging/Consumers/UpsertPositionConsumer.cs))
- `LoggerConfiguration()...CreateLogger()` Serilog-ში ([DependencyInjection.cs:44](Presentation/Presentation.WebApi/Extensions/DependencyInjection.cs#L44))
- Fluent Builder MinIO-სთვის: `new PutObjectArgs().WithBucket(...).WithObject(...)...` ([DocumentService.cs:30-35](Infrastructure/Infrastructure.Documents/Implementations/DocumentService.cs#L30-L35))
- FluentValidation Rule Builder — `RuleFor(...).NotEmpty()...WithMessage(...)` ([CreateEmployeeCommand.cs:86-101](Core/Core.Application/Interactors/Commands/CreateEmployeeCommand.cs#L86-L101))

### 2.3 Abstract Factory (გამარტივებული)
[WcfServiceConfiguration.ImplementationFactory<T>](Infrastructure/Infrastructure.Messaging/Helpers/WcfServiceConfiguration.cs#L19-L39) — configuration-ის მიხედვით ქმნის WCF კლიენტებს.

### 2.4 Singleton (DI მართული)
- `IObservable<QueueItemDto>`, `IObserver<QueueItemDto>`, `Channel<QueueItemDto>` — სინგლტონებად დარეგისტრირებული ([DependencyInjection.cs:116-128](Presentation/Presentation.WebApi/Extensions/DependencyInjection.cs#L116-L128))
- `TimeProvider.System` ([DependencyInjection.cs:31](Presentation/Presentation.WebApi/Extensions/DependencyInjection.cs#L31))

---

## 3. სტრუქტურული პატერნები (Structural)

### 3.1 Repository
სპეციფიური რეპოზიტორები, მხოლოდ საჭირო მეთოდებით (ზოგადი ბაზური კლასის გარეშე): [IEmployeeRepository](Core/Core.Application/Interfaces/Repositories/IEmployeeRepository.cs), [IPositionRepository](Core/Core.Application/Interfaces/Repositories/IPositionRepository.cs), [IEventRepository](Core/Core.Application/Interfaces/Repositories/IEventRepository.cs)

### 3.2 Adapter
- [HrPortalServices](Infrastructure/Infrastructure.Messaging/RequestServices/HrPortalServices.cs) — გარე HTTP API-ს ადაპტირებს `IHrPortalServices` ინტერფეისზე
- [DocumentService](Infrastructure/Infrastructure.Documents/Implementations/DocumentService.cs) — MinIO SDK-ს ფარავს `IDocumentService` ინტერფეისით
- [MessagingServices](Infrastructure/Infrastructure.Messaging/Producers/MessagingServices.cs) — Kafka-ს ფარავს `IMessagingService` ინტერფეისით

### 3.3 Facade
- `DependencyInjection` სტატიკური კლასები ([Application](Core/Core.Application/DependencyInjection.cs), [Persistence](Infrastructure/Infrastructure.Persistence/DependencyInjection.cs), [Documents](Infrastructure/Infrastructure.Documents/DependencyInjection.cs), [Messaging](Infrastructure/Infrastructure.Messaging/DependencyInjection.cs)) — თითო ფენის რეგისტრაცია ერთი მეთოდით

### 3.4 Decorator
- **Middleware Pipeline** ASP.NET Core-ში ([Program.cs:34-54](Presentation/Presentation.WebApi/Program.cs#L34-L54))
- **DelegatingHandler**: [CorrelationDelegatingHandler](Infrastructure/Infrastructure.Messaging/Helpers/CorrelationDelegatingHandler.cs) — HTTP კლიენტს გადააწერს `x-correlation-id` ჰედერს

### 3.5 Proxy
WCF Service Reference [PropertyGetterServiceReference/Reference.cs](Infrastructure/Infrastructure.Messaging/Connected%20Services/PropertyGetterServiceReference/Reference.cs) — გენერირებული proxy კლასი.

### 3.6 Extension Object (C# extension methods)
[CollectionsExtensions](Core/Core.Shared/Exceptions/CollectionsExtensions.cs), [ConfigurationExtensions](Core/Core.Shared/Exceptions/ConfigurationExtensions.cs), [DateTimeExtensions](Core/Core.Shared/Extensions/DateTimeExtensions.cs), [HttpClientExtension](Core/Core.Shared/Exceptions/HttpClientExtension.cs), [EfCoreExtensions](Infrastructure/Infrastructure.Persistence/EfCoreExtensions.cs)

---

## 4. ქცევითი პატერნები (Behavioral)

### 4.1 Chain of Responsibility
- ASP.NET Core middleware pipeline ([ExceptionHandler.cs](Presentation/Presentation.WebApi/Extensions/Middlewares/ExceptionHandler.cs), [RequestResponseLogging.cs](Presentation/Presentation.WebApi/Extensions/Middlewares/RequestResponseLogging.cs))
- HTTP `DelegatingHandler` ჯაჭვი

### 4.2 Strategy
- Compression providers (`Gzip`/`Brotli`) ([DependencyInjection.cs:102-103](Presentation/Presentation.WebApi/Extensions/DependencyInjection.cs#L102-L103))
- [GetMaxMinBySalaryQuery](Core/Core.Application/Interactors/Queries/GetMaxMinBySalaryQuery.cs#L25-L28) — `IsRequiredMax` ფლაგით სხვადასხვა LINQ სტრატეგია

### 4.3 Observer / Publish-Subscribe
- **Rx.NET**: `ReplaySubject<QueueItemDto>`, `IObservable<>`/`IObserver<>` ([CheckoutCommand1.cs](Core/Core.Application/Interactors/Commands/CheckoutCommand1.cs) + [LongRunningTask1Worker.cs](Presentation/Presentation.WebApi/Workers/LongRunningTask1Worker.cs))
- **Notification handler-ები**: [UpsertPositionNotification](Core/Core.Application/Interactors/Notifications/UpsertPositionNotification.cs), [CheckoutProcessCommand](Core/Core.Application/Interactors/Notifications/CheckoutProcessCommand.cs) — worker-ები და Kafka consumer საკუთარ scope-ში პირდაპირ იძახებენ `Handler.Handle(...)`-ს
- **Kafka producer/consumer** ([MessagingServices.cs](Infrastructure/Infrastructure.Messaging/Producers/MessagingServices.cs), [UpsertPositionConsumer.cs](Infrastructure/Infrastructure.Messaging/Consumers/UpsertPositionConsumer.cs))

### 4.4 Command
თითო use case-ში `Request` (ბრძანების ობიექტი) და მისი `Handler` — Command pattern ([CreateEmployeeCommand.Request](Core/Core.Application/Interactors/Commands/CreateEmployeeCommand.cs#L14)). `Request`-ები [IRequest / IRequest&lt;T&gt;](Core/Core.Application/Interactors/IRequest.cs)-ს, handler-ები კი [IRequestHandler](Core/Core.Application/Interactors/IRequestHandler.cs)-ს იმპლემენტაციას აკეთებენ — მარკერ ინტერფეისებია, runtime-ზე არაფერს აკეთებს (მხოლოდ `Handle`-ის სიგნატურას აფიქსირებს).

### 4.5 Template Method
- `BackgroundService.ExecuteAsync()` ([Worker.cs](Presentation/Presentation.Worker/Worker.cs), [UpsertPositionConsumer.cs](Infrastructure/Infrastructure.Messaging/Consumers/UpsertPositionConsumer.cs))
- [Aggregate.When()](Core/Core.Domain/Basics/Aggregate.cs#L9) — abstract template method

### 4.6 Memento (Event Sourcing-ის ნაწილი)
[Aggregate.Load()](Core/Core.Domain/Basics/Aggregate.cs#L19-L27) — state-ის აღდგენა მოვლენებიდან.

### 4.7 Specification / Predicate
[IEmployeeRepository.CheckAsync(Expression<Func<Employee, bool>>)](Core/Core.Application/Interfaces/Repositories/IEmployeeRepository.cs#L20), [IPositionRepository.CheckAsync](Core/Core.Application/Interfaces/Repositories/IPositionRepository.cs#L11) — predicate-ით ფილტრაცია.

### 4.8 Producer-Consumer
- `System.Threading.Channels.Channel<T>` ([CheckoutCommand2](Core/Core.Application/Interactors/Commands/CheckoutCommand2.cs) → [LongRunningTask2Worker](Presentation/Presentation.WebApi/Workers/LongRunningTask2Worker.cs))
- Kafka

### 4.9 Iterator
LINQ-ის გამოყენება ფართოდ, `IEnumerable<T>` ყველგან.

### 4.10 Visitor / Configuration (EF Core)
[IEntityTypeConfiguration<Employee>](Infrastructure/Infrastructure.Persistence/Configurations/EmployeeConfiguration.cs) — `Configure(EntityTypeBuilder)` ფაქტიურად Visitor-ის გამოყენებაა.

---

## 5. Cross-Cutting Concerns პატერნები

### 5.1 Dependency Injection (IoC)
ნაგებია Microsoft.Extensions.DependencyInjection-ზე. ჩანს `AddScoped`, `AddSingleton`, `AddTransient`, `AddHostedService`, `AddHttpClient` ფართოდ.

Handler-ები რეგისტრირდება [AddHandlers](Core/Core.Application/DependencyInjection.cs#L24)-ში და კონტროლერის მეთოდებს `[FromServices]` პარამეტრით მიეწოდება (method injection, მაგ. [EmployeesController](Presentation/Presentation.WebApi/Controllers/EmployeesController.cs)).

### 5.2 Service Locator (Anti-pattern, მაგრამ გამოყენებულია)
- [CreateEmployeeCommand.Handler](Core/Core.Application/Interactors/Commands/CreateEmployeeCommand.cs#L44-L50) — `services.GetRequiredService<...>()` კონსტრუქტორში
- [DocumentService](Infrastructure/Infrastructure.Documents/Implementations/DocumentService.cs#L16-L20)
- [Worker](Presentation/Presentation.Worker/Worker.cs#L13-L18)

(კონსტრუქტორ-ინექციასთან შერეული მიდგომა.)

### 5.3 Options Pattern
- `Configure<IpRateLimitOptions>(...)` ([DependencyInjection.cs:83](Presentation/Presentation.WebApi/Extensions/DependencyInjection.cs#L83))
- `Configure<GzipCompressionProviderOptions>`, `Configure<BrotliCompressionProviderOptions>`
- `MinioConfig` ([Models/MinioConfig.cs](Infrastructure/Infrastructure.Documents/Models/MinioConfig.cs))

### 5.4 Filter / Attribute (AOP-style)
- [ActionLoggingAttribute](Presentation/Presentation.WebApi/Extensions/Attributes/ActionLoggingAttribute.cs), [SkipRequestLogging/SkipResponseLogging](Presentation/Presentation.WebApi/Extensions/Attributes/ActionLoggingAttribute.cs#L6-L10)
- [ApiKeyAuthenticationAttribute](Presentation/Presentation.WebApi/Extensions/Attributes/ApiKeyAuthenticationAttribute.cs)
- [FluentValidationFilter](Presentation/Presentation.WebApi/Extensions/Services/FluentValidationFilter.cs)

### 5.5 Lazy Initialization
- `IMemoryCache.GetOrCreateAsync(...)` ([PositionsController.cs:15-19](Presentation/Presentation.WebApi/Controllers/PositionsController.cs#L15-L19))

### 5.6 Optimistic Concurrency
[EmployeeConfiguration.cs:35](Infrastructure/Infrastructure.Persistence/Configurations/EmployeeConfiguration.cs#L35) — `IsConcurrencyToken()` და [EmployeeRepository.UpdateAsync](Infrastructure/Infrastructure.Persistence/Implementations/EmployeeRepository.cs#L46) — `Version` შემოწმება.

### 5.7 Soft Delete
[EmployeeConfiguration.cs:38](Infrastructure/Infrastructure.Persistence/Configurations/EmployeeConfiguration.cs#L38) — `HasQueryFilter(x => !x.DeletedAt.HasValue)`.

### 5.8 DTO / Mapping
- [GetEmployeeDto](Core/Core.Application/DTOs/GetEmployeeDto.cs), [SetEmployeeDto](Core/Core.Application/DTOs/SetEmployeeDto.cs)
- **Mapster** კონფიგურაცია — [MapperConfig](Core/Core.Application/Commons/MapperConfig.cs)

### 5.9 Correlation ID (Distributed tracing)
[CorrelationMiddleware](Presentation/Presentation.WebApi/Extensions/Middlewares/CorrelationMiddleware.cs) + [CorrelationDelegatingHandler](Infrastructure/Infrastructure.Messaging/Helpers/CorrelationDelegatingHandler.cs).

### 5.10 Background Worker / Hosted Service
- [Worker](Presentation/Presentation.Worker/Worker.cs) (cron-based)
- [UpsertPositionConsumer](Infrastructure/Infrastructure.Messaging/Consumers/UpsertPositionConsumer.cs) (Kafka)
- [LongRunningTask1Worker](Presentation/Presentation.WebApi/Workers/LongRunningTask1Worker.cs) (Rx subscription)
- [LongRunningTask2Worker](Presentation/Presentation.WebApi/Workers/LongRunningTask2Worker.cs) (Channel reader)

### 5.11 Scheduler
[SlimTaskScheduler](Core/Core.Shared/SlimTaskScheduler.cs) — Cronos-ის ბაზაზე cron-style scheduling.

### 5.12 Deconstruction
[Employee.Deconstruct](Core/Core.Domain/Models/Employee.cs#L43-L58), [Position.Deconstruct](Core/Core.Domain/Models/Position.cs#L19-L29) — C# deconstruction pattern.

### 5.13 Nested Type Grouping (Vertical Slice-ის სტილი)
ყველა Use Case-ში `abstract class`-ის შიგნით ერთად დევს: `Request`, `Handler`, `Validator`, `Response` (მაგ. [CreateEmployeeCommand.cs](Core/Core.Application/Interactors/Commands/CreateEmployeeCommand.cs)). ეს არის ლოკალური "vertical slice" სტრუქტურა.

### 5.14 Result Shaping / Dynamic Object
[DataShaper.ShapeAs](Core/Core.Shared/Helpers/DataShaper.cs) — `ExpandoObject`-ით კლიენტის მოთხოვნილი ველების ფორმირება.

---

## 6. გადასახედი / შესაცვლელი მომენტები

- **Service Locator** გამოყენებულია რამდენიმე ადგილას — საუკეთესო პრაქტიკაა მისი თავიდან აცილება და სუფთა კონსტრუქტორ-ინექციის გამოყენება.
- [DataContext](Infrastructure/Infrastructure.Persistence/DataContext.cs)-ში დაკომენტებული Audit/EventSourcing ლოგიკა — ჩანს რომ მიდგომის შერჩევა მიმდინარეობს.

---

ჯამში — ეს არის ერთერთი ყველაზე "კატალოგისებური" .NET პროექტი რაც შემხვედრია. თითქმის ყველა მნიშვნელოვანი .NET-ის enterprise პატერნი წარმოდგენილია მკაფიო, ნათელი იმპლემენტაციით.
