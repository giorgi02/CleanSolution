using Core.Application.Interfaces.Services;
using Core.Domain.Basics;
using Core.Domain.Models;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;
internal class DataContext : DbContext
{
    public DbSet<Employee> Employes => Set<Employee>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<LogEvent> LogEvents => Set<LogEvent>();


    private readonly IActiveUserService _user;
    public DataContext(DbContextOptions<DataContext> options, IActiveUserService user)
        : base(options) => _user = user;

    #region SaveChanges -ების გადატვირთვა
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            this.Audition(entry);

        return base.SaveChangesAsync(cancellationToken);
    }
    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            this.Audition(entry);

        return base.SaveChanges();
    }
    private void Audition(EntityEntry<AuditableEntity> entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                //entry.Entity.DateCreated = DateTime.Now;
                entry.Entity.CreatedBy = _user.UserId;
                break;
            case EntityState.Modified:
                entry.Entity.Version++;

                entry.Entity.DateUpdated = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _user.UserId;

                // არ შეიცვლება ქვემოთ ჩამოთვლილი ველები
                entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                entry.Property(nameof(AuditableEntity.DateCreated)).IsModified = false;

                entry.Property(nameof(AuditableEntity.DeletedBy)).IsModified = false;
                entry.Property(nameof(AuditableEntity.DateDeleted)).IsModified = false;

                // ცვლილებების ლოგირება
                AppendEvents(entry);
                break;
            case EntityState.Deleted:
                entry.State = EntityState.Unchanged;

                // შეიცვლება მხოლოდ ქვემოთ ჩამოთვლილი ველები
                entry.Entity.DateDeleted = DateTime.UtcNow;
                entry.Entity.DeletedBy = _user.UserId;

                // ცვლილებების ლოგირება
                AppendEvents(entry);
                break;
        }
    }

    // EventSource ის შენახვა
    private void AppendEvents(EntityEntry<AuditableEntity> entry)
    {
        // ცვლილებების ლოგირება
        Dictionary<string, object> @events = new();

        foreach (var item in entry.Properties.Where(x => x.IsModified))
            @events.Add(item.Metadata.Name, item.OriginalValue!);

        this.LogEvents.Add(new(entry.Entity, 1, @events));
    }
    #endregion


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
    }
}