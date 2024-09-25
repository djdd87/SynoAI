using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Models.Configuration;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .HasMany(x => x.ZonePoints)
            .WithOne(x => x.Zone)
            .HasForeignKey(x => x.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.ZoneTargets)
            .WithOne(x => x.Zone)
            .HasForeignKey(x => x.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.ZoneTimeRanges)
            .WithOne(x => x.Zone)
            .HasForeignKey(x => x.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}