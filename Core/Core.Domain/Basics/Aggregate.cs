namespace Core.Domain.Basics;
public abstract class Aggregate : AuditableEntity
{
    private readonly IList<object> _events = new List<object>();
    public override Guid Id { get; set; } = Guid.NewGuid();


    protected abstract void When(object @event);
    public abstract Type GetEventType(string eventName);

    public void Apply(object @event)
    {
        When(@event);

        _events.Add(@event);
    }

    public void Load(long version, IEnumerable<object> history)
    {
        Version = version;

        foreach (var e in history)
        {
            When(e);
        }
    }

    public object[] GetChanges() => _events.ToArray();
}