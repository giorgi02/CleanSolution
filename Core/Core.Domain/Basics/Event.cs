using Core.Domain.Enums;
using System.Text.Json;

namespace Core.Domain.Basics;
public record class Event<TAggregate> where TAggregate : IAggregateRoot
{
    public Guid Id { get; private set; }
    public EventType EventType { get; private set; }
    public DateTime TimeStamp { get; private set; }
    public string AggregateType { get; private set; }

    public Guid AggregateId { get; private set; }
    //public int Version { get; set; }
    public Dictionary<string, object> Data { get; init; }

    public Event(EventType eventType, Guid aggregateId)
    {
        this.Id = Guid.NewGuid();
        this.TimeStamp = DateTime.Now;
        this.AggregateType = nameof(TAggregate);
        this.EventType = eventType;
        this.AggregateId = aggregateId;
    }
    public Event(EventType eventType, Guid aggregateId, object data) : this(eventType, aggregateId)
    {
        var json = JsonSerializer.Serialize(data);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        if (dictionary == null) throw new Exception("object is empty");

        this.Data = dictionary;
    }

    public void Deconstruct(out Guid id, out EventType eventType, out DateTime timeStamp, out string aggregateType, out Guid aggregateId, out Dictionary<string, object> data)
    {
        id = this.Id;
        eventType = this.EventType;
        timeStamp = this.TimeStamp;
        aggregateType = this.AggregateType;
        aggregateId = this.AggregateId;
        data = this.Data;
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
