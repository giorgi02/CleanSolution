using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
internal class LogEventConfguration : IEntityTypeConfiguration<LogEvent>
{
    public void Configure(EntityTypeBuilder<LogEvent> builder)
    {
        builder.ToTable("Events", "log");

        builder.Property(x => x.AggregateType).HasMaxLength(200);
        builder.Property(x => x.EventType).HasMaxLength(200);
    }
}