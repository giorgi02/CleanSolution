using Core.Domain.Models;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Models;

namespace Infrastructure.Persistence;

internal class DataContext(DbContextOptions<DataContext> options)
    : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Position> Positions => Set<Position>();
    internal DbSet<LogEvent> LogEvents => Set<LogEvent>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
    }

    //#region SaveChanges -ების გადატვირთვა
    //private readonly IActiveUserService _user;
    //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    //{
    //    foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
    //        this.Audition(entry);

    //    return base.SaveChangesAsync(cancellationToken);
    //}
    //public override int SaveChanges()
    //{
    //    foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
    //        this.Audition(entry);

    //    return base.SaveChanges();
    //}
    //private void Audition(EntityEntry<AuditableEntity> entry)
    //{
    //    switch (entry.State)
    //    {
    //        case EntityState.Added:
    //            entry.Entity.DateCreated = DateTime.Now;
    //            entry.Entity.CreatedBy = _user.UserId;
    //            break;
    //        case EntityState.Modified:
    //            entry.Entity.Version++;

    //            entry.Entity.DateUpdated = DateTime.Now;
    //            entry.Entity.UpdatedBy = _user.UserId;

    //            // არ შეიცვლება ქვემოთ ჩამოთვლილი ველები
    //            entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
    //            entry.Property(nameof(AuditableEntity.DateCreated)).IsModified = false;

    //            entry.Property(nameof(AuditableEntity.DeletedBy)).IsModified = false;
    //            entry.Property(nameof(AuditableEntity.DateDeleted)).IsModified = false;

    //            // ცვლილებების ლოგირება
    //            AppendEvents(entry);
    //            break;
    //        case EntityState.Deleted:
    //            entry.State = EntityState.Unchanged;

    //            // შეიცვლება მხოლოდ ქვემოთ ჩამოთვლილი ველები
    //            entry.Entity.DateDeleted = DateTime.Now;
    //            entry.Entity.DeletedBy = _user.UserId;

    //            // ცვლილებების ლოგირება
    //            AppendEvents(entry);
    //            break;
    //    }
    //}

    //// EventSource ის შენახვა
    //private void AppendEvents(EntityEntry<AuditableEntity> entry)
    //{
    //    // ცვლილებების ლოგირება
    //    Dictionary<string, object> @events = [];

    //    foreach (var item in entry.Properties.Where(x => x.IsModified))
    //        @events.Add(item.Metadata.Name, item.OriginalValue!);

    //    this.LogEvents.Add(new(entry.Entity, 1, @events));
    //}
    //#endregion
}