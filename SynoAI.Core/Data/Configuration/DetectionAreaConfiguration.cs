using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class DetectionAreaConfiguration : IEntityTypeConfiguration<DetectionArea>
{
    public void Configure(EntityTypeBuilder<DetectionArea> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .HasMany(x => x.DetectionPoints)
            .WithOne(x => x.DetectionArea)
            .HasForeignKey(x => x.DetectionAreaId);

        builder
            .HasMany(x => x.DetectionTimeRanges)
            .WithOne(x => x.DetectionArea)
            .HasForeignKey(x => x.DetectionAreaId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}