using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .HasMany(x => x.ZonePoints)
            .WithOne(x => x.Zone)
            .HasForeignKey(x => x.ZoneId);

        builder
            .HasMany(x => x.ZoneTimeRanges)
            .WithOne(x => x.Zone)
            .HasForeignKey(x => x.ZoneId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}