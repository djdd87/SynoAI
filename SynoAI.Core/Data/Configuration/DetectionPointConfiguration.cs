using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class DetectionPointConfiguration : IEntityTypeConfiguration<DetectionPoint>
{
    public void Configure(EntityTypeBuilder<DetectionPoint> builder)
    {
        builder
            .HasKey(x => x.Id);
    }
}