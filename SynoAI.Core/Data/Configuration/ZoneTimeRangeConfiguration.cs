using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class ZoneTimeRangeConfiguration : IEntityTypeConfiguration<ZoneTimeRange>
{
    public void Configure(EntityTypeBuilder<ZoneTimeRange> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}