namespace Core.Domain.Basics;

/// <summary>
/// Entity with audit columns, optimistic concurrency (Version) and soft delete (DeletedAt).
/// All of these are maintained by DataContext.SaveChanges; application code never sets them by hand
/// (except Version, which the client sends back on update).
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public virtual int Version { get; set; }

    public virtual DateTime CreatedAt { get; set; }
    public virtual string CreatedBy { get; set; } = null!;

    public virtual DateTime? UpdatedAt { get; set; }
    public virtual string? UpdatedBy { get; set; }

    public virtual DateTime? DeletedAt { get; set; }
    public virtual string? DeletedBy { get; set; }
}
