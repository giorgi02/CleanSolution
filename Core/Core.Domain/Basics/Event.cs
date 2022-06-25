namespace Core.Domain.Basics;
public abstract record class Event
{
    public int Id { get; init; }
}
