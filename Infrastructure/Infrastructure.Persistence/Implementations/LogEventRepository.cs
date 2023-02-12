using Core.Application.Interfaces.Repositories;
using Core.Domain.Basics;
using Infrastructure.Persistence.Models;
using System.Text.Json;

namespace Infrastructure.Persistence.Implementations;
internal class LogEventRepository : IEventRepository
{
    private readonly DataContext _context;
    public LogEventRepository(DataContext context) => _context = context;

    public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate, new()
    {
        var lastVesion = _context.LogEvents.Where(x => x.AggregateId == aggregate.Id).Max(x => (int?)x.Version) ?? -1;
        foreach (var item in aggregate.GetChanges())
        {
            var @event = new LogEvent(aggregate, ++lastVesion, item);
            _context.LogEvents.Add(@event);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate, new()
    {
        var events = await _context.LogEvents.Where(x => x.AggregateId == aggregateId).ToListAsync();

        return RestoreHistory<TAggregate>(events);
    }

    public async Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId, long version) where TAggregate : Aggregate, new()
    {
        var events = await _context.LogEvents.Where(x => x.AggregateId == aggregateId && x.Version <= version).ToListAsync();

        return RestoreHistory<TAggregate>(events);
    }
    public async Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId, DateTime timeStamp) where TAggregate : Aggregate, new()
    {
        var events = await _context.LogEvents.Where(x => x.AggregateId == aggregateId && x.TimeStamp <= timeStamp).ToListAsync();

        return RestoreHistory<TAggregate>(events);
    }

    private static TAggregate? RestoreHistory<TAggregate>(IEnumerable<LogEvent> events) where TAggregate : Aggregate, new()
    {
        if (!events.Any()) return null;

        var aggregate = new TAggregate();
        aggregate.Load(
            events.Max(x => x.Version),
            events.OrderBy(x => x.Version).Select(@event => JsonSerializer.Deserialize(@event.Data, aggregate.GetEventType(@event.EventType))!)
            );

        return aggregate;
    }
}