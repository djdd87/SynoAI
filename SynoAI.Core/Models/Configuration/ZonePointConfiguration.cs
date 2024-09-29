using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Models.Configuration;

public class ZonePointConfiguration : IEntityTypeConfiguration<ZonePoint>
{
    public void Configure(EntityTypeBuilder<ZonePoint> builder)
    {
        builder
            .HasKey(x => x.Id);
    }
}