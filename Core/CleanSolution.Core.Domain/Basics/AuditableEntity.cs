using System;

namespace CleanSolution.Core.Domain.Basics
{
    public abstract class AuditableEntity : BaseEntity
    {
        public virtual DateTime DateCreated { get; set; } = DateTime.Now;
        public virtual Guid? CreatedBy { get; set; }

        public virtual DateTime? DateUpdated { get; set; }
        public virtual Guid? UpdatedBy { get; set; }

        public virtual DateTime? DateDeleted { get; set; }
        public virtual Guid? DeletedBy { get; set; }
    }
}
