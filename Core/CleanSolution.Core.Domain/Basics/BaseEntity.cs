using System;

namespace CleanSolution.Core.Domain.Basics
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; set; }
    }
}
