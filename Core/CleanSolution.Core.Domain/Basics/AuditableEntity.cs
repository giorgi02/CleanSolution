using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Functions;
using CleanSolution.Core.Domain.Helpers;
using System;
using System.Collections.Generic;

namespace CleanSolution.Core.Domain.Basics
{
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
                var @event = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(e.EventBody);
                When(@event);
            }
        }

        protected void When(Dictionary<string, object> @event)
        {
            foreach (var item in @event)
            {
                var property = this.GetType().GetProperty(item.Key);

                var value = item.Value.ToString().ConvertStringTo(property.PropertyType);
                property.SetValue(this, value);
            }
        }
    }
}
