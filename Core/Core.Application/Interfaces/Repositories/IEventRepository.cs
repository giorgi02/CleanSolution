using Core.Domain.Basics;

namespace Core.Application.Interfaces.Repositories;

public interface IEventRepository
{
    Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate, new();

    Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId) where TAggregate : Aggregate, new();
    Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId, int version) where TAggregate : Aggregate, new();
    Task<TAggregate?> LoadAsync<TAggregate>(Guid aggregateId, DateTime timeStamp) where TAggregate : Aggregate, new();
}