using Core.Domain.Basics;
using System.Text.Json;

namespace Infrastructure.Persistence.Models;
internal class LogEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimeStamp { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = null!;
    public int Version { get; set; }
    public string EventType { get; set; } = null!;
    public string Data { get; set; } = null!;


    public LogEvent() { }
    public LogEvent(Aggregate aggregate, int version, object @event)
    {
        this.TimeStamp = DateTime.Now;
        this.AggregateId = aggregate.Id;
        this.AggregateType = aggregate.GetType().Name;
        this.Version = version;
        this.EventType = @event.GetType().Name;
        this.Data = JsonSerializer.Serialize(@event);
    }
    public LogEvent(AuditableEntity aggregate, int version, object @event)
    {
        this.TimeStamp = DateTime.Now;
        this.AggregateId = aggregate.Id;
        this.AggregateType = aggregate.GetType().Name;
        this.Version = version;
        this.EventType = @event.GetType().Name;
        this.Data = JsonSerializer.Serialize(@event);
    }
}