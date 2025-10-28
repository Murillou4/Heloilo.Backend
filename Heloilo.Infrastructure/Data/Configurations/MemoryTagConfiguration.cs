using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class MemoryTagConfiguration : IEntityTypeConfiguration<MemoryTag>
{
    public void Configure(EntityTypeBuilder<MemoryTag> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.TagName)
            .IsRequired()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(mt => new { mt.MemoryId, mt.TagName })
            .IsUnique();

        // Relationships
        builder.HasOne(mt => mt.Memory)
            .WithMany(m => m.Tags)
            .HasForeignKey(mt => mt.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


