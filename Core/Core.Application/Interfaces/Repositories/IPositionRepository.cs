using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;
public interface IPositionRepository : IRepository<Guid, Position> { }