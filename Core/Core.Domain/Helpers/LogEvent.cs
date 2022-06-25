using Core.Domain.Basics;

namespace Core.Domain.Helpers;
public class LogEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ObjectType { get; private set; }
    public Guid ObjectId { get; private set; }
    public string? EventBody { get; init; }
    public int Version { get; private set; }
    public DateTime ActTime { get; private set; } = DateTime.Now;


    public LogEvent(AuditableEntity aggregate)
        : this(aggregate.GetType().Name, aggregate.Id, aggregate.Version) { }

    private LogEvent(string objectType, Guid objectId, int version)
    {
        this.ObjectType = objectType;
        this.ObjectId = objectId;
        this.Version = version;
    }


    public void Deconstruct(out Guid id, out string objectType, out Guid objectId, out string? eventBody, out int version, out DateTime actTime)
    {
        id = this.Id;
        objectType = this.ObjectType;
        objectId = this.ObjectId;
        eventBody = this.EventBody;
        version = this.Version;
        actTime = this.ActTime;
    }
}
