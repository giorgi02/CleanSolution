using CleanSolution.Core.Domain.Basics;
using EventStore.ClientAPI;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CleanSolution.Infrastructure.EventSourcing
{
    // todo: კეთდება EventSourcing https://dev.to/ahmetkucukoglu/event-sourcing-with-asp-net-core-01-store-3k04
    internal class AggregateRepository
    {
        private readonly IEventStoreConnection eventStore;

        public AggregateRepository(IEventStoreConnection eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task SaveAsync<T>(T aggregate) where T : BaseAggregate, new()
        {
            var events = aggregate.GetChanges()
                .Select(@event => 
                new EventData(
                    Guid.NewGuid(),
                    @event.GetType().Name,
                    true,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                    Encoding.UTF8.GetBytes(@event.GetType().FullName)))
                .ToArray();

            if (!events.Any())
            {
                return;
            }

            var streamName = GetStreamName(aggregate, aggregate.Id);

            var result = await this.eventStore.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);
        }

        public async Task<T> LoadAsync<T>(Guid aggregateId) where T : BaseAggregate, new()
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(aggregateId));

            var aggregate = new T();
            var streamName = GetStreamName(aggregate, aggregateId);

            var nextPageStart = 0L;

            do
            {
                var page = await this.eventStore.ReadStreamEventsForwardAsync(
                    streamName, nextPageStart, 4096, false);

                if (page.Events.Length > 0)
                {
                    aggregate.Load(
                        page.Events.Last().Event.EventNumber,
                        page.Events.Select(@event => JsonSerializer.Deserialize(Encoding.UTF8.GetString(@event.OriginalEvent.Data), Type.GetType(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata)))
                        ).ToArray());
                }

                nextPageStart = !page.IsEndOfStream ? page.NextEventNumber : -1;
            } while (nextPageStart != -1);

            return aggregate;
        }

        private string GetStreamName<T>(T type, Guid aggregateId) => $"{type.GetType().Name}-{aggregateId}";
    }
}

