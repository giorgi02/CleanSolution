# Domain and persistence

## Contents
1. Entities and base classes
2. Value objects and enums
3. What DataContext does for AuditableEntity
4. EF Core configuration
5. Repositories
6. Paging, filtering, sorting
7. Concurrency and multi-entity writes
8. Seeds and temporal tables
9. Migrations
10. Event sourcing (optional)

---

## 1. Entities and base classes

Entities live in `Core.Domain/Models`, namespace `Core.Domain.Models`. Pick the base class by need:

| Base | Gives | Use for |
|---|---|---|
| `BaseEntity` | `Guid Id` | reference/lookup data without history (positions, categories) |
| `AuditableEntity` | + `Version`, `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By` | business entities: audit trail, soft delete, optimistic concurrency (default choice) |
| `Aggregate` | + event list, `Apply/When/Load` | only when the change history itself is a requirement (section 10) |

```csharp
using Core.Domain.Basics;
using Core.Domain.Enums;

namespace Core.Domain.Models;

public class Employee : AuditableEntity
{
    public string PrivateNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }

    public Language Language { get; set; }
    public string[] Phones { get; set; } = [];
    public Address? Address { get; set; }

    public Guid? PositionId { get; set; }
    public Position? Position { get; set; }
    public string? PictureName { get; set; }


    public Employee(string privateNumber, string firstName, string lastName, DateTime birthDate, Gender gender)
    {
        PrivateNumber = privateNumber;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Gender = gender;
        Language = Language.None;
    }

    public void SetLanguages(IEnumerable<Language> languages) =>
        Language = languages.Aggregate(Language.None, (all, language) => all | language);
}
```

Rules:
- The constructor takes every required (non-nullable) value, so a new entity is never half-initialized. EF Core uses the same
  constructor (parameter names match property names).
- Properties are public get/set (EF-friendly, matches the template). Logic that keeps several fields consistent goes into a
  method on the entity (`SetLanguages`, `Approve`, `ChangeAddress`), not into handlers.
- Nullable reference types are meaningful: `string?` = optional column, `string` = required.
- Relations: navigation **plus** an explicit FK property (`Guid? PositionId`) - lets handlers set a relation by id and makes
  queries simpler. Collections as `ICollection<T> Items { get; set; } = [];`.
- Money is `decimal`. Dates without time are `DateTime` mapped to `date`. Ids are `Guid`; leave `Id` unset on create and EF
  generates it (sequential GUID) - set it explicitly only for seeds or ids that come from another system.
- No data annotations, no EF types, no references beyond `Core.Shared`. Never set audit columns or `DeletedAt` by hand.
- Optional `Deconstruct(out ...)` overloads are allowed for convenient tuple-style reads (the template uses them).

## 2. Value objects and enums

Value object = immutable `record class` with a positional constructor, mapped as an owned type (columns in the owner's table):

```csharp
namespace Core.Domain.Models;

public record class Address(string City, string Street);
```

Enums in `Core.Domain/Enums`, explicit values (they are stored as `int`; never renumber):

```csharp
public enum Gender { Female = 0, Male = 1 }

[Flags]
public enum Language
{
    None = 0,
    Georgian = 1, English = 2, Russian = 4, German = 8, French = 16, Chinese = 32,
    All = Georgian | English | Russian | German | French | Chinese
}
```

Multi-select values use a `[Flags]` enum with `None = 0` and powers of two; requests carry `ICollection<Language>`, the entity
folds them into one value, queries test with `(x.Language & language) == language`.

## 3. What DataContext does for AuditableEntity

`DataContext.SaveChanges/SaveChangesAsync` (from the skeleton) handles every `AuditableEntity` entry:

| Entry state | Effect |
|---|---|
| Added | `CreatedAt` = UTC now, `CreatedBy` = `IActiveUserService.UserId` (or `"anonymous"`), `Version` = 0 |
| Modified | `UpdatedAt/By` set, `Version` + 1; Created*/Deleted* columns are never written by an update |
| Deleted | **soft delete**: the row stays, only `DeletedAt/By` and `Version` are written; owned value objects are kept |

Consequences:
- Delete = `context.Set<X>().Remove(entity)` + `SaveChangesAsync` - never set `DeletedAt` yourself.
- The soft-delete query filter (added by `ConfigureAuditable()`) hides deleted rows everywhere. Use
  `.IgnoreQueryFilters()` only for explicit history/admin reads.
- Times are UTC. `DataContext` needs `IActiveUserService` and `TimeProvider` from DI - every host that uses Persistence
  registers both (`SystemActiveUserService` in non-HTTP hosts).
- In a solution whose `DataContext` lacks this logic (the reference repo has it commented out), `Remove` is a hard delete and
  audit columns stay empty - port the skeleton's `DataContext` before relying on soft delete or `Version`.

## 4. EF Core configuration

One `internal sealed class XConfiguration : IEntityTypeConfiguration<X>` per entity in `Infrastructure.Persistence/Configurations`.
`DataContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly`, so new configurations are picked up automatically.

Existing solutions may differ - check before relying on it:
- If `OnModelCreating` lists configurations one by one (the reference repo does), **add yours to that list** or it is silently
  ignored. Switching that solution to assembly scanning is a separate change to propose: it also activates configurations
  that were never applied (the reference repo's `LogEventConfguration`) and changes the next migration.
- If `EfCoreExtensions` has no `ConfigureAuditable()`, copy it from `assets/skeleton/Infrastructure/Infrastructure.Persistence/EfCoreExtensions.cs`
  (or write its four mappings inline).

```csharp
using Core.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ConfigureAuditable(); // Version concurrency token, audit column lengths, soft-delete filter

        builder.Property(x => x.PrivateNumber).HasMaxLength(11).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BirthDate).HasColumnType("date");
        builder.Property(x => x.PictureName).HasMaxLength(300);

        // unique among non-deleted rows, so a soft-deleted record does not block re-creating it
        builder.HasIndex(x => x.PrivateNumber).IsUnique().HasFilter("[DeletedAt] IS NULL");

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.City).HasMaxLength(100);
            address.Property(x => x.Street).HasMaxLength(100);
        });

        builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId);
    }
}
```

Checklist per configuration:
- Every `string` has `HasMaxLength` (otherwise `nvarchar(max)`), plus `IsRequired()` when non-nullable.
- `decimal` money: `.HasPrecision(18, 2)`. Date-only: `.HasColumnType("date")`.
- Natural keys get a unique index; on soft-deleted entities add `.HasFilter("[DeletedAt] IS NULL")`.
- `string[]` / `List<int>` properties need no configuration: EF Core stores primitive collections as JSON. (The reference repo
  joins with `';'` through a value converter without a `ValueComparer`, which breaks change detection - don't copy it.)
- `ConfigureAuditable()` first for every `AuditableEntity`. Don't call `HasQueryFilter` again afterwards (a second unnamed
  filter replaces the soft-delete one); combine conditions in one filter if you need more.
- `#region` blocks to group sections (indexes, conversions, owned types) are fine - the template uses them.
- Prefer explicit `Include` in repositories over `Navigation(...).AutoInclude()`; use AutoInclude only for a small navigation
  that literally every query needs.

`DataContext` exposes sets as expression-bodied properties: `public DbSet<Employee> Employees => Set<Employee>();`.
Nothing but sets, model building and the auditing override belongs in `DataContext`.

## 5. Repositories

Interface in `Core.Application/Interfaces/Repositories`, implementation in `Infrastructure.Persistence/Implementations`,
registered scoped in `AddPersistenceLayer`. Only the methods the use cases call; parameters are primitives/domain types; return
domain entities or `Pagination<TEntity>` (never `IQueryable`, never DTOs).

```csharp
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Shared;
using System.Linq.Expressions;

namespace Core.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber, string? firstName, string? lastName,
        Gender? gender, Language language, CancellationToken cancellationToken = default);

    Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CheckAsync(Expression<Func<Employee, bool>> predicate, CancellationToken cancellationToken = default);
}
```

```csharp
using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Shared;
using Core.Shared.Exceptions;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Implementations;

internal sealed class EmployeeRepository(DataContext context) : IEmployeeRepository
{
    private IQueryable<Employee> Including() => context.Employees.Include(x => x.Position);


    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        context.Employees.Add(employee);
        await context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    // tracked on purpose: update handlers modify the returned entity and call UpdateAsync
    public Task<Employee?> ReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        Including().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string? privateNumber, string? firstName, string? lastName,
        Gender? gender, Language language, CancellationToken cancellationToken = default) =>
        Including()
            .Where(x =>
                (privateNumber == null || x.PrivateNumber == privateNumber) &&
                (firstName == null || x.FirstName == firstName) &&
                (lastName == null || x.LastName == lastName) &&
                (gender == null || x.Gender == gender) &&
                (language == Language.None || (x.Language & language) == language))
            .OrderByDescending(x => x.CreatedAt)
            .ToPaginatedAsync(pageIndex, pageSize, cancellationToken);

    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return employee;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw OperationForbiddenException.Create("The record was changed by someone else. Reload it and try again.");
        }
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await context.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (employee is null) return 0;

        context.Employees.Remove(employee); // soft delete (DataContext)
        return await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CheckAsync(Expression<Func<Employee, bool>> predicate, CancellationToken cancellationToken = default) =>
        context.Employees.AnyAsync(predicate, cancellationToken);
}
```

```csharp
// AddPersistenceLayer
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
```

Rules:
- Each write method ends with `SaveChangesAsync` - there is no unit of work to call later.
- `ReadAsync(id)` returns a tracked entity (so updates work); list/search methods return untracked data (`ToPaginatedAsync`
  applies `AsNoTracking`; add `.AsNoTracking()` to other read-only lists).
- Never expose `IQueryable` or accept `Expression` for arbitrary querying beyond `CheckAsync`; add a named method instead.
- `FindAsync` bypasses `Include` - use `FirstOrDefaultAsync` with the `Including()` query when navigations are needed.

## 6. Paging, filtering, sorting

- `ToPaginatedAsync(pageIndex, pageSize, ct)` (1-based) - always after an `OrderBy`, or pages are nondeterministic.
- Optional filters: `(param == null || x.Prop == param)` inside one `Where`. Flags: bitwise test. Text search: a separate
  `SearchAsync(pageIndex, pageSize, text, ct)` matching the few columns that make sense (`x.FirstName.Contains(text)`).
- Client-chosen sorting (`?orderBy=lastName,birthDate desc`) only when asked: add `System.Linq.Dynamic.Core` to Persistence and
  an `ApplySort` extension in `EfCoreExtensions` that whitelists property names via reflection, builds
  `"LastName ascending, BirthDate descending"` and calls `source.OrderBy(expression)`. Never pass the raw string to `OrderBy`,
  and don't call `source.Any()` first (it executes the query).
- Client-chosen fields (`?fields=id,firstName`): `DataShaper.ShapeAs(fields)` on the DTOs in the controller, only when asked.

## 7. Concurrency and multi-entity writes

- `AuditableEntity.Version` is the concurrency token (`ConfigureAuditable()`). DTOs expose it; update requests send it back.
- Handler compares `entity.Version != request.Version` → `OperationForbiddenException` (409). The repository additionally maps
  `DbUpdateConcurrencyException` (someone saved in between) to the same exception.
- A use case that must change several entities atomically gets **one repository method** that applies all changes and calls
  `SaveChangesAsync` once (EF wraps one `SaveChanges` in a transaction). If it truly spans repositories, use
  `await using var tx = await context.Database.BeginTransactionAsync(ct)` inside a dedicated repository method - never expose
  transactions to Core.Application.

## 8. Seeds and temporal tables

```csharp
namespace Infrastructure.Persistence.Seeds;

internal static class PositionSeed
{
    internal static readonly Position[] Seeding =
    [
        new("Developer", 2000) { Id = Guid.Parse("53c161b8-415e-402d-80c1-da798aa3d047") },
        new("Tester", 1000) { Id = Guid.Parse("90c8f00f-f112-4929-b630-c174899e9f17") }
    ];
}

// in PositionConfiguration
builder.HasData(PositionSeed.Seeding);
```

Seed ids are fixed literals (a new `Guid.NewGuid()` each build would rewrite the seed in every migration). `HasData` bypasses
`SaveChanges`, so seeding an `AuditableEntity` requires literal values for `CreatedAt`/`CreatedBy` as well - prefer seeding
`BaseEntity` reference data.

Reference data whose history matters (prices, salaries) can be a SQL Server temporal table:
`builder.ToTable(x => x.IsTemporal());` and read history in the repository with `context.Positions.TemporalAll()` /
`TemporalAsOf(utcTime)`.

## 9. Migrations

```bash
dotnet ef migrations add Add<Entity> --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi --output-dir Migrations
dotnet ef database update --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi
```

- Name migrations after the change (`AddDepartment`, `AddEmployeeDepartment`), one migration per feature.
- Requires the `dotnet-ef` tool and `Microsoft.EntityFrameworkCore.Design` in the **startup** project (the skeleton has it; the
  reference repo does not, so `dotnet ef` fails there until the package is added to Presentation.WebApi).
- If `dotnet ef --version` fails, don't install tools silently; tell the user to run `dotnet tool install --global dotnet-ef`
  (add `--ignore-failed-sources` when a private NuGet feed is unreachable) and continue with the rest of the work.
- Read the generated migration before keeping it: renamed columns show up as drop + add (data loss) - use `RenameColumn`
  by editing the *new* migration only if the user agrees. Never edit applied migrations or the model snapshot by hand.
- A migration is generated from the model; it doesn't need a running database. `database update` does.

## 10. Event sourcing (optional)

Only when the user wants the full change history of an aggregate rebuilt from events:
- Aggregate inherits `Aggregate`, implements `When(object @event)` (state transitions) and `GetEventType(string name)`;
  commands call `aggregate.Apply(new SomethingHappened(...))`.
- `IEventRepository` (`SaveAsync`, `LoadAsync(id)`, `LoadAsync(id, version)`, `LoadAsync(id, timeStamp)`) is implemented in
  Persistence with a `LogEvent` table (`Models/LogEvent.cs`, `Configurations/LogEventConfiguration.cs` → table `log.Events`).
- Register `services.AddScoped<IEventRepository, LogEventRepository>()` (missing in the reference repo).
- For plain auditing the soft-delete/audit columns, or a temporal table, are enough - don't reach for event sourcing.
