---
name: clean-solution
description: Architecture, conventions and libraries of the CleanSolution .NET 10 Clean Architecture template (Core.Domain/Application/Shared, Infrastructure.Persistence/Messaging/Documents, Presentation.WebApi/Worker/Mcp) - use-case classes with nested Request/Handler/Validator called directly (no MediatR), specific EF Core repositories, FluentValidation, Mapster, exception-to-ProblemDetails mapping, API versioning, Serilog, Kafka, MinIO, cron workers, MCP tools. Use this skill whenever you work in a solution with this layout, or the user asks to create a new .NET solution / microservice "like CleanSolution" or "with our architecture", to add an entity, CRUD, command, query, endpoint, controller, repository, migration, Kafka producer/consumer, HTTP or WCF client, file upload, background job or MCP tool, or to review / refactor C# code against these conventions - even when the skill is not named.
---

# CleanSolution architecture

CleanSolution is a .NET 10 Clean Architecture template: three layers (Core, Infrastructure, Presentation), use cases written as
small self-contained classes, and a fixed set of libraries. This skill describes the **corrected standard**: how the template is
meant to work, with the known defects of the reference repository fixed. When the reference repo and this skill disagree,
follow this skill (the known deviations are listed in `references/review-checklist.md`).

## Decide what kind of task this is

| Task | Go to |
|---|---|
| Create a new solution | [Creating a new solution](#creating-a-new-solution) |
| Add an entity / use case / endpoint / repository | [Adding a feature](#adding-a-feature) + `references/use-cases.md`, `references/domain-and-persistence.md`, `references/web-api.md` |
| Add Kafka, HTTP client, WCF, MinIO upload, background queue, cron job, MCP tool | `references/integrations.md` |
| Review or refactor code against the conventions | [Code review](#code-review) + `references/review-checklist.md` |
| Any question about where something belongs | `references/architecture.md` |

Before changing an existing solution, read the code you are about to extend: the layer's `DependencyInjection.cs`, one
existing use case of the same kind, the controller next to yours. Match the names that solution already uses (for example the
reference repo calls its method `AddApplicatonLayer` and its cron key `CrosExpression`) - renaming them is a separate change you
can suggest, not something to slip into a feature.

## The architecture in one picture

```
Presentation.WebApi ─┐  Presentation.Worker ─┐  Presentation.Mcp ─┐     composition roots (hosts)
                     ▼                       ▼                    ▼
        Infrastructure.Persistence   Infrastructure.Messaging   Infrastructure.Documents   adapters (EF Core, Kafka/HTTP/WCF, MinIO)
                     └──────────────────────┬────────────────────┘
                                            ▼
                                     Core.Application    use cases, DTOs, interfaces (ports), validators, mapping
                                            ▼
                                       Core.Domain       entities, value objects, enums
                                            ▼
                                       Core.Shared       exceptions, Pagination<T>, extensions, helpers
```

Dependencies only point down. Infrastructure projects reference **only** Core.Application (never each other, never Presentation).
Core.Application knows nothing about ASP.NET Core, EF Core, Kafka or MinIO - it declares interfaces (`IEmployeeRepository`,
`IDocumentService`, `IMessagingService`) and Infrastructure implements them. This is what lets the same use case run from a
controller, a Kafka consumer, a cron worker or an MCP tool.

## Rules that hold everywhere (and why)

1. **No mediator, no AutoMapper.** MediatR was removed on purpose (commercial license, hidden indirection). A use case is a
   concrete `Handler` class that callers resolve from DI and call `Handle(...)` on directly. Mapping uses Mapster (`.Adapt<T>()`).
   Do not reintroduce MediatR, AutoMapper, pipeline behaviors or a dispatcher.
2. **Use case = one abstract container class** (`CreateEmployeeCommand`, `GetEmployeesQuery`, `UpsertPositionNotification`)
   with nested `Request`, `Handler`, optional `Validator`, optional `Response`. Everything about one use case lives in one file.
   Details: `references/use-cases.md`.
3. **Specific repositories, no generic `Repository<T>` and no `UnitOfWork`.** The author deprecated both. Each repository exposes
   only what use cases need; each write method calls `SaveChangesAsync` itself.
4. **Constructor injection only** (primary constructors). Never inject `IServiceProvider` into handlers, repositories or
   services. The only places that resolve from a provider are composition code that must create a scope: hosted services,
   Kafka consumers, the `SlimTaskScheduler` callback, MVC filters, MCP tools that validate.
5. **Errors are exceptions, never hand-built responses.** Throw `EntityNotFoundException` (400, deliberately not 404),
   `OperationForbiddenException` (409, concurrency/state conflicts) or `ApiValidationException` (400). `ExceptionHandler`
   middleware turns them into RFC 9110 `ValidationProblemDetails`; everything else becomes a 500 without details.
6. **Validation is FluentValidation in the nested `Validator`.** HTTP requests are validated automatically by the global
   `FluentValidationFilter`. Every other entry point (consumer, worker, MCP tool) must call
   `services.ValidateOrThrowAsync(request, ct)` before the handler - otherwise its input is never validated.
7. **Core.Application stays framework-free.** No `IFormFile`, `HttpContext`, `DbContext`, Kafka or MinIO types. Uploaded files
   reach use cases as `FileContent` (controller converts with `formFile.ToFileContent()`).
8. **`CancellationToken` flows through every async call**, from the controller action down to EF/HTTP calls.
9. **English everywhere, no localization.** Comments, XML docs, log templates and user-facing messages are plain English written
   inline (`"Position was not found."`). Do not add `IStringLocalizer` or resx files.
10. **Configuration is explicit and fails fast.** Every key read in code exists in appsettings under the same path (check both
    files when you add one); required values are read with `configuration.GetString("Section:Key")`, which throws when missing.
    Secrets (passwords, keys, tokens) go to user-secrets / environment variables, never into committed appsettings.
11. **Register everything you add**: handlers in `AddHandlers()` (Application), repositories in `AddPersistenceLayer`,
    adapters in their layer's `Add{Layer}Layer`, background loops (Kafka consumers, queue workers, cron jobs) only in the one
    host that runs them. A missing registration compiles fine and fails at runtime, so treat registration as part of writing
    the class. Every host is a complete composition root: it calls every `Add{Layer}Layer` whose ports any handler uses,
    because Development validates all registered handlers at startup.
12. **Structured logging only**: message templates with `{Name}` / `{@Object}` placeholders, never string interpolation.
    Never log secrets, tokens or file contents (use `[SkipRequestLogging]` / `[SkipResponseLogging]`).

Style: file-scoped namespaces equal to the folder path, `sealed` classes unless designed for inheritance, Infrastructure
implementations are `internal` (only `DependencyInjection` is public), nullable reference types respected, collection
expressions (`[]`), `async`/`await` end to end.

## Creating a new solution

Do not hand-write the skeleton - the bundled script produces a build-verified one.

1. **Collect the inputs.** Solution name (PascalCase, e.g. `InventoryHub`), parent folder, and the optional modules. The core
   is always included (Core.Shared, Core.Domain, Core.Application, Infrastructure.Persistence, Presentation.WebApi). Ask about
   the optional ones unless the request already answers it:
   - `messaging` - Infrastructure.Messaging: Kafka producers/consumers, typed HTTP clients, (WCF on demand)
   - `documents` - Infrastructure.Documents: MinIO file storage
   - `worker` - Presentation.Worker: cron-scheduled jobs
   - `mcp` - Presentation.Mcp: MCP server exposing use cases as tools
   - JWT authentication is optional and **off by default** (the code is always there); pass `--jwt` only when asked.
2. **Run the script** (Python 3, stdlib only; it builds the solution at the end):
   ```bash
   python <skill-dir>/scripts/scaffold.py --name InventoryHub --output <parent-folder> --modules messaging,worker [--jwt]
   ```
3. **If the build fails**, fix the cause (usually an unreachable private NuGet feed: retry `dotnet restore --ignore-failed-sources`).
4. **Then add the requested features** with the feature workflow below. In a fresh solution the first controller should also
   add `global using Core.Application.DTOs;`, `...Interactors.Commands;`, `...Interactors.Queries;` to
   `Presentation.WebApi/Extensions/DependencyInjection.cs` (those namespaces only exist once the first use case does).
5. **Report** what was created, which modules are in, and the next steps (connection string / secrets, first migration).

The scaffold contains: per-layer `DependencyInjection.cs`, marker interfaces `IRequest`/`IRequestHandler`, exceptions,
`Pagination<T>`, `DataContext` with auditing + soft delete + optimistic concurrency, `ExceptionHandler`, `FluentValidationFilter`,
`ActionLoggingAttribute`, correlation id, Serilog (Seq/Graylog), API versioning, OpenAPI + Scalar + Swagger UI, health checks,
rate limiting, CORS, response compression, and the selected modules. Look at `assets/skeleton/` for the canonical source of
any of these files.

## Adding a feature

Work inside-out so every layer compiles against the one below it. For "add entity X with CRUD":

1. **Domain** - `Core.Domain/Models/X.cs` (+ enums in `Enums/`, value objects as `record class`). Constructor takes the
   required fields; inherit `AuditableEntity` when it needs audit columns, soft delete or concurrency (the default for
   business entities), `BaseEntity` for simple reference data. → `references/domain-and-persistence.md`
2. **Port** - `Core.Application/Interfaces/Repositories/IXRepository.cs` with only the methods the use cases need.
3. **Persistence** - `Configurations/XConfiguration.cs` (max lengths, required, indexes, owned types, concurrency token,
   soft-delete query filter), `DbSet` in `DataContext`, `Implementations/XRepository.cs`, register it in
   `AddPersistenceLayer`.
4. **Application** - `DTOs/GetXDto.cs`; Mapster config only for non-1:1 members; use cases in `Interactors/Commands` and
   `Interactors/Queries` (`CreateXCommand`, `UpdateXCommand`, `DeleteXCommand`, `GetXQuery`, `GetXsQuery`), each with its
   `Validator` when it has input; register every handler in `AddHandlers()`. → `references/use-cases.md`
5. **Presentation** - `Controllers/XsController.cs` with versioned route, `[FromServices]` handlers, `ProducesResponseType`
   triplet, named routes. → `references/web-api.md`
6. **Migration** - `dotnet ef migrations add AddX --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi --output-dir Migrations`.
   If `dotnet ef` is not installed, do not install global tools on your own - tell the user the command
   (`dotnet tool install --global dotnet-ef`) and leave the migration step for them. Never hand-write or hand-edit migrations
   or the model snapshot.
7. **Verify** - `dotnet build` the solution; fix all errors and new warnings. Then re-read the checklist in
   `references/review-checklist.md` against your diff (registrations, config keys, cancellation tokens, validators).

For a single use case or endpoint, do only the steps that apply - but always the registration and the build.

## Code review

Read `references/review-checklist.md` and check the code (a diff, a project or the whole solution) against it. Report findings
grouped by severity with `file:line`, the rule, why it matters, and the concrete fix. Do not report style preferences that are
not in the checklist as defects. If asked to fix, fix blockers and majors first and rebuild.

## Reference files

| File | Read when |
|---|---|
| `references/architecture.md` | Where does X go; project/folder/namespace map; DI composition; full library list with versions; what not to use |
| `references/use-cases.md` | Writing commands, queries, notifications, validators, DTOs, mapping, error handling |
| `references/domain-and-persistence.md` | Entities, value objects, enums, EF configuration, repositories, pagination, auditing, soft delete, concurrency, migrations, seeds |
| `references/web-api.md` | Controllers, routes, status codes, file upload/download, caching, pipeline order, auth (JWT/API key), logging, health, OpenAPI |
| `references/integrations.md` | Kafka producer/consumer, typed HttpClient, WCF, MinIO, in-process background queue, cron worker, MCP tools |
| `references/review-checklist.md` | Reviews; final self-check after a feature; known deviations in the reference repo |
| `assets/skeleton/` | Canonical source of every infrastructure file the scaffold generates |
| `scripts/scaffold.py` | Creating a new solution (`--help` for options) |
