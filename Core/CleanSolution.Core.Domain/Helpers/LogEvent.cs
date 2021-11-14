using CleanSolution.Core.Domain.Basics;

namespace CleanSolution.Core.Domain.Helpers;
public class LogEvent
{
    public Guid Id { get; set; }
    public string ObjectType { get; set; }
    public Guid ObjectId { get; set; }
    public string EventBody { get; set; }
    public int Version { get; set; }
    public DateTime ActTime { get; set; }


    private LogEvent() { /* for deserialization & ORMs */}
    public LogEvent(AuditableEntity aggregate)
        : this()
    {
        this.ObjectType = aggregate.GetType().Name;
        this.ObjectId = aggregate.Id;
        this.Version = aggregate.Version;
        this.ActTime = DateTime.Now;
    }

    public void Deconstruct(out Guid id, out string objectType, out Guid objectId, out string eventBody, out int version, out DateTime actTime)
    {
        id = this.Id;
        objectType = this.ObjectType;
        objectId = this.ObjectId;
        eventBody = this.EventBody;
        version = this.Version;
        actTime = this.ActTime;
    }
}
