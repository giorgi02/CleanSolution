﻿using System;

namespace $safeprojectname$.Basics
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; init; }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(this.Id);
            return hash.ToHashCode();
        }
    }
}