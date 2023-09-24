namespace Core.Domain.Basics;
public abstract record class Event<TAggregate> where TAggregate : Aggregate;