using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Models.Configuration;

public class CameraConfiguration : IEntityTypeConfiguration<Camera>
{
    public void Configure(EntityTypeBuilder<Camera> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .HasMany(x => x.Zones)
            .WithOne(x => x.Camera)
            .HasForeignKey(x => x.CameraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}