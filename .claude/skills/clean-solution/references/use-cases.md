# Use cases (Interactors)

## Contents
1. Anatomy
2. Request shapes
3. Templates: create, update, delete, get by id, paged list, notification
4. Handler rules
5. Validator rules
6. DTOs and Mapster
7. Errors
8. Registration and callers

---

## 1. Anatomy

One file per use case in `Core.Application/Interactors/{Commands|Queries|Notifications}/`, namespace
`Core.Application.Interactors.{Commands|Queries|Notifications}`. The outer class is `abstract` (never instantiated, it only
groups the parts):

```csharp
public abstract class CreateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto> { ... }                 // input; IRequest<TResponse> or IRequest
    public sealed class Handler(...) : IRequestHandler<Request, GetEmployeeDto> { ... }   // logic
    public sealed class Validator : AbstractValidator<Request> { ... }                    // input rules (when there is input)
    public sealed record class Response { ... }                                           // only when no existing DTO fits
}
```

`IRequest`, `IRequest<TResponse>`, `IRequestHandler<TRequest>`, `IRequestHandler<TRequest, TResponse>` are marker interfaces in
`Core.Application/Interactors` - they fix the `Handle(TRequest, CancellationToken)` signature and nothing else. There is no
dispatcher: whoever needs the use case gets `CreateEmployeeCommand.Handler` from DI and calls `Handle`.

- **Commands** change state (create/update/delete/process). They return the affected DTO, a nested `Response`, or nothing.
- **Queries** only read. They never write, publish messages or call `SaveChanges`.
- **Notifications** are handlers triggered by something other than an HTTP request: Kafka consumers, cron jobs, in-process
  queues. Same shape, `IRequest` without a response.

## 2. Request shapes

| Input comes from | Shape |
|---|---|
| Route values / simple flags / nothing | `public record struct Request(Guid EmployeeId) : IRequest<GetEmployeeDto>;` |
| JSON body, form, or query string | `public sealed record class Request : IRequest<...>` with `{ get; set; }` properties |
| Route id + body (PUT) | class shape + `internal Guid EmployeeId { get; private set; }` and a public `SetEmployeeId(Guid)` returning `this` |
| A file upload | class shape + `internal FileContent? Picture { get; private set; }` and a public `SetPicture(FileContent?)` |

Rules:
- Model-bound properties are settable and reference types are nullable (`string?`): binding can leave them null, the
  validator guarantees them, and the handler then uses `!` (`request.FirstName!`).
- Initialize collections (`= []`, `= new HashSet<Language>()`), so a missing field means "empty", not `null`.
- `internal` + `Set...` keeps route ids and files out of the JSON body and out of the OpenAPI schema. Because MVC filters run
  before the action, the validator never sees these values - check them in the handler if needed.
- Value objects (`Address`) and enums from Core.Domain are fine in requests; entities are not. `IFormFile` is never in a request.

## 3. Templates

### 3.1 Create (with optional file and an event)

```csharp
using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;
using Core.Domain.Models;
using Mapster;

namespace Core.Application.Interactors.Commands;

public abstract class CreateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto>
    {
        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Language> Languages { get; set; } = new HashSet<Language>();
        public string[] Phones { get; set; } = [];
        public Address? Address { get; set; }
        public Guid PositionId { get; set; }

        // attached by the controller from IFormFile; never bound from the body
        internal FileContent? Picture { get; private set; }

        public Request SetPicture(FileContent? picture)
        {
            Picture = picture;
            return this;
        }
    }


    public sealed class Handler(
        IEmployeeRepository employeeRepository,
        IPositionRepository positionRepository,
        IDocumentService documents,
        IMessagingService messaging) : IRequestHandler<Request, GetEmployeeDto>
    {
        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var position = await positionRepository.ReadAsync(request.PositionId, cancellationToken)
                ?? throw new EntityNotFoundException("Position was not found.", nameof(request.PositionId));

            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender)
            {
                Phones = request.Phones,
                Address = request.Address,
                Position = position
            };
            employee.SetLanguages(request.Languages);

            if (request.Picture is not null)
                employee.PictureName = await documents.SaveAsync(request.Picture, cancellationToken);

            await employeeRepository.CreateAsync(employee, cancellationToken);

            // side effects only after the data is saved
            await messaging.EmployeeCreated(employee, cancellationToken);

            return employee.Adapt<GetEmployeeDto>();
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator(TimeProvider timeProvider)
        {
            RuleFor(x => x.PrivateNumber)
                .NotEmpty().WithMessage("Private number is required.")
                .Length(11).WithMessage("Private number must be 11 characters long.")
                .Matches("^[0-9]*$").WithMessage("Private number must contain digits only.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name must be at most 100 characters long.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name must be at most 100 characters long.");

            RuleFor(x => x.Gender).IsInEnum().WithMessage("Gender is invalid.");

            RuleFor(x => x.BirthDate)
                .Must(birthDate => IsWorkingAge(birthDate, timeProvider))
                .WithMessage("Employee must be between 18 and 100 years old.");

            RuleFor(x => x.PositionId).NotEmpty().WithMessage("Position is required.");
        }

        private static bool IsWorkingAge(DateTime birthDate, TimeProvider timeProvider)
        {
            var today = timeProvider.GetLocalNow().Date;
            return birthDate <= today.AddYears(-18) && birthDate > today.AddYears(-100);
        }
    }
}
```

### 3.2 Update with optimistic concurrency

The client sends back the `Version` it read. Load the tracked entity, compare versions, change it, save. Do **not** build a new
entity and copy it over the tracked one with `SetValues` (that tries to change the key and silently skips navigations and owned
types - this is a known bug in the reference repo).

```csharp
public abstract class UpdateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto>
    {
        internal Guid EmployeeId { get; private set; }
        public int Version { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Language> Languages { get; set; } = new HashSet<Language>();
        public string[] Phones { get; set; } = [];
        public Address? Address { get; set; }
        public Guid PositionId { get; set; }

        public Request SetEmployeeId(Guid employeeId)
        {
            EmployeeId = employeeId;
            return this;
        }
    }


    public sealed class Handler(IEmployeeRepository employeeRepository, IPositionRepository positionRepository)
        : IRequestHandler<Request, GetEmployeeDto>
    {
        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.ReadAsync(request.EmployeeId, cancellationToken)
                ?? throw new EntityNotFoundException("Employee was not found.", nameof(request.EmployeeId));

            if (employee.Version != request.Version)
                throw OperationForbiddenException.Create("The employee was changed by someone else. Reload it and try again.", nameof(request.Version));

            var position = await positionRepository.ReadAsync(request.PositionId, cancellationToken)
                ?? throw new EntityNotFoundException("Position was not found.", nameof(request.PositionId));

            employee.FirstName = request.FirstName!;
            employee.LastName = request.LastName!;
            employee.Phones = request.Phones;
            employee.Address = request.Address;
            employee.Position = position;
            employee.SetLanguages(request.Languages);

            await employeeRepository.UpdateAsync(employee, cancellationToken); // DataContext bumps Version, sets UpdatedAt/By

            return employee.Adapt<GetEmployeeDto>();
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Version).GreaterThanOrEqualTo(0).WithMessage("Version is invalid.");
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(100);
            RuleFor(x => x.PositionId).NotEmpty().WithMessage("Position is required.");
        }
    }
}
```

When an update has real invariants (status transitions, totals), move the assignments into an entity method
(`order.Approve(by)`) and call that instead.

### 3.3 Delete (soft)

```csharp
public abstract class DeleteEmployeeCommand
{
    public record struct Request(Guid EmployeeId) : IRequest;


    public sealed class Handler(IEmployeeRepository employeeRepository) : IRequestHandler<Request>
    {
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            // Remove() becomes a soft delete in DataContext for AuditableEntity
            var deleted = await employeeRepository.DeleteAsync(request.EmployeeId, cancellationToken);

            if (deleted == 0)
                throw new EntityNotFoundException("Employee was not found.", nameof(request.EmployeeId));
        }
    }
}
```

### 3.4 Get by id

```csharp
public abstract class GetEmployeeQuery
{
    public record struct Request(Guid EmployeeId) : IRequest<GetEmployeeDto>;


    public sealed class Handler(IEmployeeRepository repository) : IRequestHandler<Request, GetEmployeeDto>
    {
        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = await repository.ReadAsync(request.EmployeeId, cancellationToken)
                ?? throw new EntityNotFoundException("Employee was not found.", nameof(request.EmployeeId));

            return employee.Adapt<GetEmployeeDto>();
        }
    }
}
```

A missing record is an error (`EntityNotFoundException`), not a `null` response - keep this consistent across queries.

### 3.5 Paged and filtered list

```csharp
using Core.Shared;

public abstract class GetEmployeesQuery
{
    public sealed record class Request : IRequest<Pagination<GetEmployeeDto>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public ICollection<Language> Languages { get; set; } = new HashSet<Language>();
    }


    public sealed class Handler(IEmployeeRepository repository) : IRequestHandler<Request, Pagination<GetEmployeeDto>>
    {
        public async Task<Pagination<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var languages = request.Languages.Aggregate(Language.None, (all, language) => all | language);

            var employees = await repository.FilterAsync(request.PageIndex, request.PageSize,
                request.PrivateNumber, request.FirstName, request.LastName, request.Gender, languages, cancellationToken);

            return employees.Adapt<Pagination<GetEmployeeDto>>();
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1).WithMessage("Page index starts at 1.");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}
```

Pass **every** filter the request exposes to the repository (the reference repo's version silently ignores most of them).
`Adapt<Pagination<TDto>>()` works because `Pagination<T>.MetaData`'s constructor parameter names match its properties; if a
solution's `MetaData` constructor still takes `count`, Mapster leaves `TotalCount` at 0 - build the result with
`Pagination<TDto>.Create(page.Data.Adapt<List<TDto>>(), page.Meta.TotalCount, page.Meta.PageIndex, page.Meta.PageSize)` or fix
the parameter name.

Small, bounded reference lists (positions, categories) may return `IEnumerable<TDto>` without paging.

### 3.6 Notification (run by a consumer, worker or queue)

```csharp
namespace Core.Application.Interactors.Notifications;

public abstract class UpsertPositionNotification
{
    public sealed record class Request : IRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal Salary { get; set; }
    }


    public sealed class Handler(IPositionRepository repository) : IRequestHandler<Request>
    {
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            var position = await repository.ReadAsync(request.Id, cancellationToken);

            if (position is null)
                await repository.CreateAsync(new Position(request.Name!, request.Salary) { Id = request.Id }, cancellationToken);
            else
            {
                position.Name = request.Name!;
                position.Salary = request.Salary;
                await repository.UpdateAsync(position, cancellationToken);
            }
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Salary).GreaterThan(0);
        }
    }
}
```

Notifications must be idempotent when the trigger can repeat (Kafka redelivers after a crash): upsert by id, check before insert.

## 4. Handler rules

- `public sealed class Handler(...)` with a **primary constructor** taking only interfaces from `Core.Application/Interfaces`,
  `TimeProvider`, or other application services. Never `IServiceProvider`, never `DataContext`, never `HttpContext`.
- Every async call gets the `cancellationToken`. Add `cancellationToken.ThrowIfCancellationRequested()` before an expensive or
  irreversible step that follows other awaits (uploading a file, calling an external service).
- Return DTOs, never domain entities (entities leak columns, navigations and audit fields, and can loop in serialization).
- Order: load and check → change state → persist → side effects (publish, notify). Side effects after the save, so a failed
  save never emits an event.
- Construct entities explicitly through their constructor; don't `Adapt` a request into an entity.
- Handlers don't call other handlers. Shared logic goes into an entity method, a repository method, or an application service
  behind an interface.
- Use `TimeProvider` for "now"; `IActiveUserService` for "who".

## 5. Validator rules

- Nested `public sealed class Validator : AbstractValidator<Request>`; discovered automatically by `AddValidatorsFromAssembly`.
- Every command/query with input gets one. Messages are English and must make sense to a client as-is: FluentValidation's
  default text is fine for generic rules (`NotEmpty`, `MaximumLength`); write `.WithMessage("...")` for formats, ranges and
  business rules.
- Mirror the database constraints (`MaximumLength` = `HasMaxLength`, required = `IsRequired`) so violations become a 400, not a
  500 from SQL Server.
- Time-dependent rules use the injected `TimeProvider`, not `DateTime.Now`.
- Async rules with a repository (`MustAsync(async (id, ct) => await repository.CheckAsync(x => x.Id == id, ct))`) are allowed for
  cheap existence checks; anything that needs the loaded entity belongs in the handler (otherwise it is queried twice).
- Combine conditions carefully: "between 18 and 100" is `older than 18 && younger than 100` (the reference repo uses `||`, which
  accepts every date).
- HTTP: `FluentValidationFilter` runs it. Elsewhere: `await scope.ServiceProvider.ValidateOrThrowAsync(request, ct)` before
  calling the handler.

## 6. DTOs and Mapster

- `Core.Application/DTOs/Get{Entity}Dto.cs`: flat, public `{ get; set; }` properties, nullable reference types, nested DTOs for
  related entities (`GetPositionDto? Position`). Expose `Version` on DTOs of `AuditableEntity` types so clients can send it back
  on update.
- Numeric types match the entity (money is `decimal` on both sides).
- `entity.Adapt<TDto>()`, `entities.Adapt<IEnumerable<TDto>>()`, `page.Adapt<Pagination<TDto>>()`.
- Explicit maps only for computed or renamed members, in `Commons/MapperConfig.cs`:
  ```csharp
  TypeAdapterConfig<Employee, GetEmployeeDto>.NewConfig()
      .Map(dest => dest.Age, src => src.BirthDate.CalculateAge())
      .Map(dest => dest.Gender, src => src.Gender.ToString());
  ```
  (`CalculateAge` is in `Core.Shared.Extensions`; `DateTime.Now.Year - BirthDate.Year` is wrong around birthdays.)

## 7. Errors

| Situation | Throw | HTTP |
|---|---|---|
| Referenced or requested record does not exist | `new EntityNotFoundException("Position was not found.", nameof(request.PositionId))` | 400 |
| Client's `Version` is stale / state does not allow the operation | `OperationForbiddenException.Create("...", nameof(request.Version))` | 409 |
| Business rule violated (duplicate, limit exceeded, invalid combination) | `new ApiValidationException("...", nameof(request.Field))` | 400 |
| Several field errors at once | `new ApiValidationException(IDictionary<string, string[]>)` | 400 |
| External dependency failed (wrapped by the adapter) | `new ApiValidationException("<Service> is unavailable. Try again later.")` | 400 |
| Anything unexpected | let it propagate | 500, logged |

The `field` argument becomes the key in `errors` of the problem details; use `nameof(request.X)`, or omit it for a general message
(key `"messages"`). 400 for not-found is deliberate: 404 is reserved for "no such route".

## 8. Registration and callers

Register every handler in `Core.Application/DependencyInjection.cs`:

```csharp
private static void AddHandlers(this IServiceCollection services)
{
    // commands
    services.AddTransient<CreateEmployeeCommand.Handler>();
    services.AddTransient<UpdateEmployeeCommand.Handler>();
    services.AddTransient<DeleteEmployeeCommand.Handler>();

    // queries
    services.AddTransient<GetEmployeeQuery.Handler>();
    services.AddTransient<GetEmployeesQuery.Handler>();

    // notifications
    services.AddTransient<UpsertPositionNotification.Handler>();
}
```

How each entry point calls a use case:

| Caller | How |
|---|---|
| Controller | `[FromServices] CreateEmployeeCommand.Handler handler` action parameter → `await handler.Handle(request, cancellationToken)` (validated by the filter) |
| Kafka consumer / in-process queue worker | `using var scope = scopeFactory.CreateScope();` → `ValidateOrThrowAsync` → `scope.ServiceProvider.GetRequiredService<X.Handler>().Handle(...)` |
| Cron worker | inside `SlimTaskScheduler.ExecutePeriodicallyAsync(async services => ...)` - `services` is already a fresh scope |
| MCP tool | handler injected into the tool class constructor; validate first (see `integrations.md`) |
