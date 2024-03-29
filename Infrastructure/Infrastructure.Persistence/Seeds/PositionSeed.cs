﻿using Core.Domain.Models;

namespace Infrastructure.Persistence.Seeds;
internal static class PositionSeed
{
    internal readonly static Position[] Seeding =
    [
        new("პროგრამისტი", 2000) { Id = Guid.Parse("53c161b8-415e-402d-80c1-da798aa3d047") },
        new("ტესტერი", 1000) { Id = Guid.Parse("90c8f00f-f112-4929-b630-c174899e9f17") }
    ];
}