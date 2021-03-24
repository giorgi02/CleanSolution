using CleanSolution.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace $safeprojectname$.Configurations
{
    internal class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(x => x.PrivateNumber).HasMaxLength(11).IsRequired();
            builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.BirthDate).HasColumnType("date");

            #region აღწერილია: უნიკალური და არაკლასტერიზებული ინდექსები
            builder.HasIndex(x => x.PrivateNumber).IsUnique();
            #endregion

            #region აღწერილია: მასივის (Array) ტიპის ველები
            builder.Property(x => x.Phones)
                .HasMaxLength(500)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries));
            #endregion

            #region აღწერილია: საკუთრებაში მყოფი (Owned) ტიპები
            builder.OwnsOne(x => x.Address).Property(x => x.City).HasMaxLength(100);
            builder.OwnsOne(x => x.Address).Property(x => x.Street).HasMaxLength(100);
            #endregion

            builder.HasQueryFilter(x => !x.DateDeleted.HasValue);
        }
    }
}
