using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class MoodTypeConfiguration : IEntityTypeConfiguration<MoodType>
{
    public void Configure(EntityTypeBuilder<MoodType> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(mt => mt.Emoji)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(mt => mt.MoodCategory)
            .HasConversion<string>();

        builder.Property(mt => mt.Description)
            .HasMaxLength(255);

        builder.Property(mt => mt.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(mt => mt.Name)
            .IsUnique();

        builder.HasIndex(mt => new { mt.MoodCategory, mt.IsActive });
    }
}
