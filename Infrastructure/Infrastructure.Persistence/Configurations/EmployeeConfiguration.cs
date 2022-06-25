using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(x => x.PrivateNumber).HasMaxLength(11).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BirthDate).HasColumnType("date");

        builder.Navigation(x => x.Position).AutoInclude();

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

        #region Concurrency Token ველის მონიშვნა (ოპტიმისტური კონკურენცია)
        builder.Property(o => o.Version).IsConcurrencyToken(true);
        #endregion

        builder.HasQueryFilter(x => !x.DateDeleted.HasValue);
    }
}