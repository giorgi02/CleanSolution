using CleanSolution.Core.Domain.Entities;
using System;

namespace CleanSolution.Core.Application.Interfaces.Repositories
{
    public interface IPositionRepository : IRepository<Guid, Position>
    {
    }
}
