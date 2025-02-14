namespace Core.Domain.Basics;
public abstract class AuditableEntity : BaseEntity
{
    public virtual int Version { get; set; } = -1;

    public virtual DateTime DateCreated { get; set; }
    public virtual string CreatedBy { get; set; } = null!;

    public virtual DateTime? DateUpdated { get; set; }
    public virtual string? UpdatedBy { get; set; }

    public virtual DateTime? DateDeleted { get; set; }
    public virtual string? DeletedBy { get; set; }
}