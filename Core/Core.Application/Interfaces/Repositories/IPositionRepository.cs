using Core.Domain.Models;

namespace Core.Application.Interfaces.Repositories;
public interface IPositionRepository : IRepository<Guid, Position> { }