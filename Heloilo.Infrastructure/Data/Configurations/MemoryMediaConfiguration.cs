using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class MemoryMediaConfiguration : IEntityTypeConfiguration<MemoryMedia>
{
    public void Configure(EntityTypeBuilder<MemoryMedia> builder)
    {
        builder.HasKey(mm => mm.Id);

        builder.Property(mm => mm.FileBlob)
            .IsRequired()
            .HasColumnType("BLOB");

        builder.Property(mm => mm.FileType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(mm => mm.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(mm => mm.Memory)
            .WithMany(m => m.Media)
            .HasForeignKey(mm => mm.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


