# Review checklist

Use it for code reviews and as the final self-check after implementing a feature.

## Contents
1. How to review
2. Rules (with severity and how to check)
3. Report format
4. Known deviations in the CleanSolution reference repo

---

## 1. How to review

1. Establish scope: a diff (`git diff <base>...HEAD`), a project, or the whole solution.
2. Build first (`dotnet build`); errors and new warnings are findings by themselves.
3. Walk the rules below. Many can be found with a search before reading code (patterns given per rule); confirm each hit in
   context before reporting it - a pattern match is a lead, not a finding.
4. Report only real deviations from this standard, with a concrete fix. Style preferences outside this list are not defects.

Severity:
- **blocker** - breaks at runtime, corrupts data, leaks secrets, or violates the layer boundaries.
- **major** - works today but fails under realistic conditions (concurrency, errors, other hosts) or makes the next change hard.
- **minor** - convention/naming/consistency.

## 2. Rules

### A. Architecture and dependencies
| ID | Rule | Sev | How to check |
|---|---|---|---|
| A1 | Core.* never references Infrastructure.* / Presentation.*; Infrastructure.X never references Infrastructure.Y or Presentation | blocker | `ProjectReference` lines in every csproj |
| A2 | Core.Application has no ASP.NET Core / EF Core / Kafka / MinIO / WCF types or packages | blocker | grep Core.Application for `Microsoft.AspNetCore`, `IFormFile`, `HttpContext`, `DbContext`, `Confluent`, `Minio`, `System.ServiceModel` |
| A3 | Core.Domain depends only on Core.Shared; no data annotations / EF attributes | major | Core.Domain csproj, grep `\[Key\]\|\[Required\]\|\[Column` |
| A4 | No MediatR, AutoMapper, generic `Repository<T>`, `UnitOfWork`, pipeline behaviors | major | grep `MediatR\|AutoMapper\|IRepository<\|UnitOfWork\|IPipelineBehavior` |
| A5 | Every package used directly is referenced directly (no transitive reliance) | minor | compare `using` namespaces with csproj packages (e.g. `System.Reactive`) |
| A6 | Every host is a complete composition root (calls every `Add{Layer}Layer` whose ports handlers use); background loops registered in exactly one host | blocker | each `Program.cs`; run each host in Development - DI validation fails fast |

### B. Use cases
| ID | Rule | Sev | How to check |
|---|---|---|---|
| B1 | Use case = abstract container with nested `Request` / `Handler` / `Validator` / `Response`, in `Interactors/{Commands,Queries,Notifications}` | minor | file layout |
| B2 | Every handler registered in `AddHandlers()` (transient) | blocker | compare `class Handler` occurrences with `AddTransient<...Handler>` lines |
| B3 | Handlers use constructor injection of ports only - no `IServiceProvider`, `DataContext`, `HttpContext` | major | grep handlers for `IServiceProvider\|GetRequiredService\|GetService<` |
| B4 | Every request with input has a `Validator`; rules mirror DB constraints; time via `TimeProvider`; combined conditions correct | major | read validators; grep `DateTime.Now` in Application |
| B5 | Non-HTTP entry points (consumers, workers, MCP tools) call `ValidateOrThrowAsync` before the handler | major | grep `GetRequiredService<.*Handler>` and check for validation nearby |
| B6 | Handlers return DTOs, never entities; queries never write | major | handler return types; `SaveChanges`/`UpdateAsync` in Queries |
| B7 | Missing records → `EntityNotFoundException`; conflicts → `OperationForbiddenException`; rule violations → `ApiValidationException`; no hand-built error responses | major | grep `return null`, `BadRequest(`, `NotFound(` in controllers/handlers |
| B8 | `CancellationToken` passed to every async call | major | async calls without a token argument |
| B9 | Updates load the tracked entity, compare `Version`, mutate, save - never `SetValues` from a newly built entity | blocker | grep `CurrentValues.SetValues` |
| B10 | Side effects (publish, notify) after the save; handlers don't call other handlers | major | order of calls in handlers |
| B11 | List queries pass every filter they expose to the repository and are paged (`Pagination<T>`) unless the list is small and bounded | major | compare Request properties with the repository call |
| B12 | English messages, no localization infrastructure | minor | non-ASCII user-facing strings, `IStringLocalizer`, `.resx` |

### C. Persistence
| ID | Rule | Sev | How to check |
|---|---|---|---|
| C1 | Every entity has an `IEntityTypeConfiguration` that is actually applied (assembly scan, or listed in `OnModelCreating`) | blocker | `OnModelCreating` vs files in Configurations/ |
| C2 | Strings have `HasMaxLength` (+`IsRequired`); money `decimal` with precision; date-only `date` | major | configurations vs entity properties |
| C3 | `AuditableEntity` configs call `ConfigureAuditable()` (concurrency token + soft-delete filter); unique indexes on soft-deleted entities are filtered | major | grep `IsConcurrencyToken\|HasQueryFilter\|IsUnique` |
| C4 | `DataContext` implements auditing / soft delete / `Version` increment (skeleton) - otherwise `Remove` hard-deletes and concurrency is a no-op | blocker | `DataContext.SaveChanges*` overrides |
| C5 | Repositories: specific methods, each write calls `SaveChangesAsync`, no `IQueryable` exposed, `DbUpdateConcurrencyException` → `OperationForbiddenException`, read lists `AsNoTracking`, paging after `OrderBy` | major | Implementations/ |
| C6 | Repository registered scoped in `AddPersistenceLayer` | blocker | grep `AddScoped<I.*Repository` |
| C7 | Model change ⇒ new migration generated by `dotnet ef`; migrations and snapshot never hand-edited; snapshot matches the model | major | `dotnet ef migrations has-pending-model-changes` (when the tool is available) |
| C8 | Value converters for collections have a `ValueComparer` - or use EF primitive collections | minor | grep `HasConversion` |

### D. Web API
| ID | Rule | Sev | How to check |
|---|---|---|---|
| D1 | Controllers: `[ApiController]`, `[ApiVersion]`, `api/v{version:apiVersion}/[controller]` route, `sealed`, handlers via `[FromServices]`, no business logic / try-catch / validation | major | Controllers/ |
| D2 | Route ids typed (`{id:guid}`), named routes, create returns `CreatedAtAction`, background work returns `Accepted` | minor | action attributes |
| D3 | `ProducesResponseType` for success, 400 (`ValidationProblemDetails`), 500 (`ProblemDetails`), 409 where relevant | minor | action attributes |
| D4 | `IFormFile` converted with `ToFileContent()`; never passed into Core.Application | blocker | grep `IFormFile` outside Presentation |
| D5 | Pipeline order as in `web-api.md` §1; `ExceptionHandler` wraps everything after correlation/compression | major | Program.cs |
| D6 | Sensitive / large bodies marked `[SkipRequestLogging]` / `[SkipResponseLogging]`; headers (Authorization) never logged | major | logging filter and attributes |
| D7 | With JWT enabled: health and API docs endpoints `AllowAnonymous`, policies used via `[Authorize(Policy=...)]` | major | Program.cs, AddStartup |
| D8 | API-key filter applied with `[TypeFilter(typeof(ApiKeyAuthenticationAttribute))]`, constant-time comparison, non-empty key | major | grep `ApiKeyAuthentication` |

### E. Integrations and background work
| ID | Rule | Sev | How to check |
|---|---|---|---|
| E1 | Ports in Application, `internal sealed` adapters in Infrastructure; external contracts not in Core.Domain | major | Interfaces/ vs Infrastructure |
| E2 | Kafka producer is one long-lived instance (singleton), flushed/disposed; messages are explicit contracts, keyed by entity id | major | Producers/ |
| E3 | Consumers / queue workers / cron jobs: scope per item, validate, catch-log-continue per item, stop cleanly on cancellation | blocker | Consumers/, Workers/, Worker.cs |
| E4 | Typed HttpClient: base address from `ExternalServices:<Name>` (trailing `/`), relative endpoints, correlation handler, failures wrapped in `ApiValidationException`, a uniquely **named** URL health check on the same key | major | Messaging DI, RequestServices/ |
| E5 | No `async void` (e.g. `Subscribe(async ...)`), no unbounded `ReplaySubject` | blocker | grep `Subscribe(async\|ReplaySubject` |
| E6 | Hosted services depend on `IServiceScopeFactory`, not scoped services | blocker | constructor parameters of `BackgroundService` classes |

### F. Configuration and security
| ID | Rule | Sev | How to check |
|---|---|---|---|
| F1 | Every key read in code exists in appsettings under the same path (and vice versa for removed code) | blocker | grep `GetString("\|GetSection("\|GetConnectionString("` vs appsettings*.json |
| F2 | No secrets in committed appsettings (passwords, keys, tokens, real hosts with credentials) | blocker | appsettings*.json |
| F3 | Required values read with `GetString` (fail fast) or validated typed settings | minor | `configuration["..."]` uses |
| F4 | No vulnerable packages (`dotnet list package --vulnerable`) | major | build warnings NU1902/NU1903 |

### G. Logging and style
| ID | Rule | Sev | How to check |
|---|---|---|---|
| G1 | Structured log templates, no interpolation in log calls | minor | grep `Log[A-Za-z]*\(\$"` |
| G2 | File-scoped namespaces = folder path; `sealed` by default; Infrastructure implementations `internal`; primary constructors for DI | minor | |
| G3 | Names follow `architecture.md` §4 | minor | |
| G4 | No dead code: commented-out blocks, unused classes, stale `.http` files | minor | |

## 3. Report format

```markdown
## Review: <scope>

Build: <succeeded / N errors, M warnings>. Findings: <b> blockers, <m> majors, <n> minors.

### Blockers
- **[B9]** `Infrastructure/Infrastructure.Persistence/Implementations/EmployeeRepository.cs:49` - `SetValues` copies a newly
  built entity (Id = Guid.Empty) onto the tracked one; EF throws "property 'Id' is part of a key". Every PUT /employees fails.
  **Fix:** load the tracked entity in the handler, compare `Version`, assign fields, `UpdateAsync(employee)` = `SaveChangesAsync`.

### Majors
- ...

### Minors
- ...
```

Group repeated occurrences of one rule into one finding with all locations. When asked to fix: blockers first, rebuild after
each group, then report what changed and what was left (and why).

## 4. Known deviations in the CleanSolution reference repo

The repository this skill was derived from has these defects. Don't copy them into new code; when reviewing that repo, report
them. ("verified" = reproduced while building this skill.)

| Area | Location | Problem | Rule |
|---|---|---|---|
| Update | `EmployeeRepository.UpdateAsync` | `SetValues` from a new entity with empty `Id` throws (verified); would also skip `Position`/`Address` | B9 |
| MinIO | `DocumentService` | resolves concrete `MinioClient`, which `Minio.AspNetCore` does not register (only `IMinioClient`, verified); reads `Minio:BucketName`, but config has `MinioConfigs:MinioBucket:BucketName`; service locator | F1, B3 |
| HTTP client | `Infrastructure.Messaging/DependencyInjection.cs` | reads `ServiceUrls:HrPortal`; appsettings has `ExternalServices:HrPortal`. Endpoint concatenation produces `api/employees//{pn}` | F1, E4 |
| Paging | `Pagination<T>.MetaData(count, ...)` | Mapster maps `TotalCount` = 0 because the parameter is not named `totalCount` (verified) | B11 |
| Migrations | Presentation.WebApi | no `Microsoft.EntityFrameworkCore.Design` in the startup project → `dotnet ef` fails (verified); existing migration/snapshot is stale (table `Employes`) | C7 |
| Auditing | `DataContext` | audit / soft delete commented out → `CreatedBy` (required column) stays null so inserting an `Employee` fails; `Remove` hard-deletes despite the soft-delete filter; `CreatedAt` never set (but used for ordering); `Version` never incremented | C4 |
| Configurations | `DataContext.OnModelCreating` | explicit list misses `LogEventConfguration`; `LogEventRepository` not registered | C1 |
| Validation | `CreateEmployeeCommand.Validator` | age rule uses `\|\|` → accepts any date; many messages hard-coded in Georgian, others via `IStringLocalizer` | B4, B12 |
| Validation | consumers, workers | handlers called without validation | B5 |
| Consumer | `UpsertPositionConsumer` | `catch` outside the loop: first failing message stops consumption; service locator style | E3 |
| Scheduler | `SlimTaskScheduler` | rethrows → one failed run stops the worker | E3 |
| Rx worker | `LongRunningTask1Worker` | `ReplaySubject` keeps every item forever; `Subscribe(async ...)` is `async void`; `System.Reactive` only transitive | E5, A5 |
| Producer | `MessagingServices` | builds a Kafka producer per message; serializes the domain entity | E2 |
| Helpers | `JsonCompressor.Compress` | reads the buffer before the Brotli stream is flushed → truncated output | - |
| Helpers | `HttpQueryStrings` | static shared `StringBuilder` → not thread-safe | - |
| Logging | `ActionLoggingAttribute` | logs the request after the action ran, duration covers only result execution, logs all headers including `Authorization` | D6 |
| Auth | `ApiKeyAuthenticationAttribute` | needs DI but is written as a plain attribute; non-constant-time compare | D8 |
| Secrets | `appsettings*.json` | JWT key, SA password, API key committed | F2 |
| Layers | `CreateEmployeeCommand.Request.Picture` | `IFormFile` in Core.Application | A2, D4 |
| DI | `CreateEmployeeCommand.Handler`, `DocumentService`, `Worker`, `LongRunningTask1Worker` | service locator | B3 |
| Worker host | `Presentation.Worker/Program.cs` | registers one handler by hand, no layers; config key typo `CrosExpression` | A6 |
| Queries | `GetEmployeesQuery` | passes only first name and languages to `FilterAsync` (param typo `firatName`); `GetPositionQuery` returns null where `GetEmployeeQuery` throws | B11, B7 |
| Mapping | `MapperConfig`, DTOs | `Age = Now.Year - BirthDate.Year`; gender mapped to Georgian text; `Salary` is `double` on the entity and `decimal` on the DTO | B12 |
| Persistence | `EmployeeConfiguration` | unique `PrivateNumber` index ignores soft delete; `Phones` converter without `ValueComparer` | C3, C8 |
| Versioning | `AddStartup` | no `.AddMvc()`, `AssumeDefaultVersionWhenUnspecified` (analyzer warnings AV0013/AV0016) | - |
| Packages | Messaging, Persistence, Worker, WebApi | vulnerable transitive `System.Security.Cryptography.Pkcs` / `.Xml` (NU1902/NU1903) | F4 |
| Misc | various | duplicate `CollectionsExtensions` (Shared and Application); extension classes under `Core.Shared/Exceptions`; typos `AddApplicatonLayer`, `LogEventConfguration`; `CheckoutProcessCommand` in Notifications; README says .NET 9 (projects are net10.0); `.http` file targets `/weatherforecast` | G3, G4 |
