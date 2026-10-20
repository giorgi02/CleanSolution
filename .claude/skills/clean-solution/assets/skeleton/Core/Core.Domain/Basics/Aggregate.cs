namespace Core.Domain.Basics;

/// <summary>
/// Event-sourced aggregate: state changes are expressed as events (Apply), the current state is
/// rebuilt by replaying the history (Load). Use only when the history itself is a requirement.
/// </summary>
public abstract class Aggregate : AuditableEntity
{
    private readonly List<object> _events = [];
    public override Guid Id { get; set; } = Guid.NewGuid();


    protected abstract void When(object @event);
    public abstract Type GetEventType(string eventName);

    public void Apply(object @event)
    {
        When(@event);

        _events.Add(@event);
    }

    public void Load(int version, IEnumerable<object> history)
    {
        Version = version;

        foreach (var e in history)
        {
            When(e);
        }
    }

    public object[] GetChanges() => [.. _events];
}
