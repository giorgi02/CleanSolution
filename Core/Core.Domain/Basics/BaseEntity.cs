﻿using System.Text.Json;

namespace Core.Domain.Basics;
public abstract class BaseEntity
{
    public virtual Guid Id { get; set; }


    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(this.Id);
        return hash.ToHashCode();
    }

    public bool Equals(BaseEntity other) => this.Equals((object)other);

    public override bool Equals(object? other)
    {
        if (other == null || this.GetType() != other.GetType())
            return false;

        var otherEntity = other as BaseEntity;

        if (otherEntity is null) return false;

        return this.Id.Equals(otherEntity.Id) && object.ReferenceEquals(this, otherEntity);
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right) => (left, right) switch
    {
        (null, null) => true,
        ({ }, { }) => left.Equals(right),
        _ => false
    };

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);

    public override string ToString() => JsonSerializer.Serialize<object>(this);
}
