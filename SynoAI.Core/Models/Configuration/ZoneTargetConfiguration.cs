using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Models.Configuration;

public class ZoneTargetConfiguration : IEntityTypeConfiguration<ZoneTarget>
{
    public void Configure(EntityTypeBuilder<ZoneTarget> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
