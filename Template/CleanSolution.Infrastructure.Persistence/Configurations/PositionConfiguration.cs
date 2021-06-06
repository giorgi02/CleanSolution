using CleanSolution.Core.Domain.Entities;
using $safeprojectname$.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace $safeprojectname$.Configurations
{
    internal class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Salary).IsRequired();

            builder.HasData(
                PositionSeed.Developer,
                PositionSeed.Tester
                );
        }
    }
}
