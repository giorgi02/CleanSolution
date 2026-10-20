using Core.Application.Interfaces.Services;
using Core.Domain.Basics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;

internal class DataContext(DbContextOptions<DataContext> options, IActiveUserService activeUser, TimeProvider timeProvider)
    : DbContext(options)
{
    // DbSets: public DbSet<Employee> Employees => Set<Employee>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // picks up every IEntityTypeConfiguration<T> in this assembly (Configurations/ folder)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditing();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    #region Auditing, optimistic concurrency and soft delete of AuditableEntity
    private void ApplyAuditing()
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = activeUser.UserId ?? "anonymous";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    entry.Entity.Version = 0;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    entry.Entity.Version++;

                    // creation and deletion columns are never changed by an update
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.DeletedAt).IsModified = false;
                    entry.Property(x => x.DeletedBy).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // soft delete: only DeletedAt/DeletedBy/Version are written, the row stays
                    KeepOwnedValues(entry);
                    entry.State = EntityState.Unchanged;
                    entry.Property(x => x.DeletedAt).CurrentValue = now;
                    entry.Property(x => x.DeletedBy).CurrentValue = user;
                    entry.Property(x => x.Version).CurrentValue = entry.Entity.Version + 1;
                    break;
            }
        }
    }

    /// <summary>
    /// Removing an owner also marks its owned types (OwnsOne value objects) as deleted, which would
    /// null their columns. For a soft delete they must stay as they are.
    /// </summary>
    private static void KeepOwnedValues(EntityEntry entry)
    {
        foreach (var reference in entry.References)
        {
            var target = reference.TargetEntry;
            if (target is not null && target.Metadata.IsOwned() && target.State == EntityState.Deleted)
            {
                KeepOwnedValues(target);
                target.State = EntityState.Unchanged;
            }
        }
    }
    #endregion
}
