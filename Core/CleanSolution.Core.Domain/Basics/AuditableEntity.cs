using System;

namespace CleanSolution.Core.Domain.Basics
{
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// ჩანაწერის ცვლილების რიგითი ნომერი
        /// გვიცავს გაუთვალისწინებელი, განმეორებითი Update -სბისგან
        /// </summary>
        public virtual int Version { get; set; } = 1;

        public virtual DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public virtual Guid? CreatedBy { get; set; }

        public virtual DateTime? DateUpdated { get; set; }
        public virtual Guid? UpdatedBy { get; set; }

        public virtual DateTime? DateDeleted { get; set; }
        public virtual Guid? DeletedBy { get; set; }
    }
}
