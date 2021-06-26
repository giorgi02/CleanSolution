using System;
using System.Collections.Generic;
using System.Linq;

namespace CleanSolution.Core.Domain.Basics
{
    public abstract class BaseAggregate
    {
        readonly IList<object> changes = new List<object>();

        public Guid Id { get; protected set; } = Guid.Empty;
        public long Version { get; private set; } = -1;


        protected abstract void When(object @event);

        public void Apply(object @event)
        {
            When(@event);

            this.changes.Add(@event);
        }

        public void Load(long version, IEnumerable<object> history)
        {
            Version = version;

            foreach (var e in history)
            {
                When(e);
            }
        }

        public object[] GetChanges() => this.changes.ToArray();
    }
}
