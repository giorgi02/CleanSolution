# Copilot Instructions for CleanSolution

## Architecture Overview
- **Domain-Driven Design**: The solution is organized into Core (Domain, Application, Shared), Infrastructure (Persistence, Messaging, Documents), and Presentation (WebApi, Worker, Mcp) layers.
- **Data Flow**: Requests enter via Presentation (WebApi controllers), are handled by MediatR command/query handlers in Core.Application, which use repositories (Core.Application.Interfaces.Repositories) implemented in Infrastructure.Persistence.
- **Persistence**: Uses Entity Framework Core. `DataContext` in Infrastructure.Persistence configures DbSets and model mappings. Auditing and event logging are handled via commented hooks in `DataContext`.
- **Background Services**: Worker services and long-running background tasks use `BackgroundService` (see Presentation.Worker and Presentation.WebApi/Workers).
- **Messaging**: Infrastructure.Messaging provides integration with external services and message brokers. Messaging patterns are implemented via producers/consumers.
- **Documents**: Infrastructure.Documents handles file storage (e.g., Minio) via `DocumentService`.

## Key Patterns & Conventions
- **CQRS with MediatR**: All business logic is implemented as command/query handlers (e.g., `CreateEmployeeCommand.Handler`). Controllers only delegate to MediatR.
- **Dependency Injection**: All services and repositories are registered via `DependencyInjection.cs` files in each layer.
- **DTOs**: Data transfer between layers uses DTOs in Core.Application.DTOs.
- **Validation**: FluentValidation is used for request validation, with custom filters in Presentation.WebApi.Extensions.Services.
- **Exception Handling**: Centralized via custom middleware (see `ExceptionHandler.cs`).
- **Auditing**: Auditable entities inherit from `AuditableEntity` (Core.Domain.Basics). Auditing logic is prepared in `DataContext` but may be commented out.

## Developer Workflows
- **Build**: Standard .NET build (`dotnet build`).
- **Run Web API**: `dotnet run --project Presentation/Presentation.WebApi`
- **Run Worker**: `dotnet run --project Presentation/Presentation.Worker`
- **Migrations**: Use EF Core CLI tools in Infrastructure.Persistence for database migrations.
- **Tests**: (Add test project if not present; follow CQRS handler test pattern.)

## Integration Points
- **External Messaging**: See Infrastructure.Messaging for WCF and message broker integration.
- **File Storage**: Minio integration via Infrastructure.Documents.
- **API Versioning**: Controllers use Asp.Versioning.

## Examples
- **Add Employee**: `EmployeesController.Add` → `CreateEmployeeCommand.Handler` → `EmployeeRepository.CreateAsync`
- **Audit Logging**: (See commented code in `DataContext.cs` for how to enable.)

## File/Directory References
- `Core/Domain/Models/` — Domain entities
- `Core/Application/Interactors/Commands/` — Command handlers
- `Core/Application/Interactors/Queries/` — Query handlers
- `Infrastructure/Persistence/Implementations/` — Repository implementations
- `Presentation/WebApi/Controllers/` — API endpoints
- `Presentation/WebApi/Extensions/` — Middleware, filters, DI

---

If you add new features, follow the CQRS and DDD patterns above. For cross-cutting concerns (logging, validation, etc.), prefer middleware or behaviors over controller/service logic.
