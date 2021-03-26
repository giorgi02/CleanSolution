using CleanSolution.Core.Domain.Entities;
using CleanSolution.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanSolution.Infrastructure.Persistence.Configurations
{
    internal class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Salary).IsRequired();

            builder.HasData(
                SeedOfPositions.Developer,
                SeedOfPositions.Tester
                );
        }
    }
}
