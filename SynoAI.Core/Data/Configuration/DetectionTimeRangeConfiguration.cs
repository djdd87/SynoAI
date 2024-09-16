using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class DetectionTimeRangeConfiguration : IEntityTypeConfiguration<DetectionTimeRange>
{
    public void Configure(EntityTypeBuilder<DetectionTimeRange> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}