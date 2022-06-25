using Core.Domain.Extensions;
using Core.Domain.Helpers;
using System.Text.Json;

namespace Core.Domain.Basics;
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// ჩანაწერის ცვლილების რიგითი ნომერი
    /// გვიცავს გაუთვალისწინებელი, განმეორებითი Update -ებისგან
    /// </summary>
    public virtual int Version { get; set; } = 0;

    public virtual DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public virtual Guid? CreatedBy { get; set; }

    public virtual DateTime? DateUpdated { get; set; }
    public virtual Guid? UpdatedBy { get; set; }

    public virtual DateTime? DateDeleted { get; set; }
    public virtual Guid? DeletedBy { get; set; }


    public void Load(IEnumerable<LogEvent> events)
    {
        foreach (var e in events)
        {
            var @event = JsonSerializer.Deserialize<Dictionary<string, object>>(e.EventBody!);

            if (@event != null) When(@event);
        }
    }

    protected void When(Dictionary<string, object> @event)
    {
        foreach (var item in @event)
        {
            var property = this.GetType().GetProperty(item.Key);

            var value = item.Value.ToString()!.ConvertFromString(property?.PropertyType);
            property!.SetValue(this, value);
        }
    }
}
