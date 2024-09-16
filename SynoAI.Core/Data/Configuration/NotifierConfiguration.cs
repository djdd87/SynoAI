using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynoAI.Core.Data.Configuration;

public class NotifierConfiguration : IEntityTypeConfiguration<Notifier>
{
    public void Configure(EntityTypeBuilder<Notifier> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.MessageTemplate)
            .IsRequired()
            .HasMaxLength(1000);
    }
}