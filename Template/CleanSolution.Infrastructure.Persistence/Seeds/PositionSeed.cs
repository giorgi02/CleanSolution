using CleanSolution.Core.Domain.Entities;
using System;

namespace $safeprojectname$.Seeds
{
    internal static class PositionSeed
    {
        internal static readonly Position Developer = new() { Id = Guid.NewGuid(), Name = "პროგრამისტი", Salary = 2000 };
        internal static readonly Position Tester = new() { Id = Guid.NewGuid(), Name = "ტესტერი", Salary = 1000 };
    }
}
