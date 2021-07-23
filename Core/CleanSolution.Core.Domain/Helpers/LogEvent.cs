using CleanSolution.Core.Domain.Basics;
using System;

namespace CleanSolution.Core.Domain.Helpers
{
    public class LogEvent
    {
        public Guid Id { get; set; }
        public string ObjectType { get; set; }
        public Guid ObjectId { get; set; }
        public string EventBody { get; set; }
        public int Version { get; set; }
        public DateTime ActTime { get; set; }

        public LogEvent() { }
        public LogEvent(AuditableEntity aggregate)
        {
            this.ObjectType = aggregate.GetType().Name;
            this.ObjectId = aggregate.Id;
            this.Version = aggregate.Version;
            this.ActTime = DateTime.Now;
        }
    }
}
