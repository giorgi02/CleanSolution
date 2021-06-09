using System;

namespace CleanSolution.Core.Domain.Basics
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

        public bool Equals(BaseEntity other) => this.Equals((object)other);
        public override bool Equals(object other)
        {
            if (other == null || this.GetType() != other.GetType())
                return false;

            var compareTo = other as BaseEntity;

            if (ReferenceEquals(this, other as BaseEntity))
                return true;

            return compareTo != null && this.Id.Equals(compareTo.Id);
        }

        public static bool operator ==(BaseEntity left, BaseEntity right) => (left, right) switch
        {
            (null, null) => true,
            ({ }, { }) => left.Equals(right),
            _ => false
        };

        public static bool operator !=(BaseEntity left, BaseEntity right) => !(left == right);

        public override string ToString() => $"{this.GetType().Name} \"id\" : {this.Id}";
    }
}
