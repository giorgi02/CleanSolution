using CleanSolution.Core.Domain.Entities;
using System;

namespace CleanSolution.Infrastructure.Persistence.Seeds
{
    internal static class SeedOfPositions
    {
        internal static readonly Position Developer = new Position { Id = Guid.NewGuid(), Name = "პროგრამისტი", Salary = 2000 };
        internal static readonly Position Tester = new Position { Id = Guid.NewGuid(), Name = "ტესტერი", Salary = 1000 };
    }
}
