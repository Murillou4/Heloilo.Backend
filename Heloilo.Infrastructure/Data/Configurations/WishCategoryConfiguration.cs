using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class WishCategoryConfiguration : IEntityTypeConfiguration<WishCategory>
{
    public void Configure(EntityTypeBuilder<WishCategory> builder)
    {
        builder.HasKey(wc => wc.Id);

        builder.Property(wc => wc.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wc => wc.Emoji)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(wc => wc.Description)
            .HasMaxLength(255);

        builder.Property(wc => wc.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(wc => wc.Name)
            .IsUnique();

        builder.HasIndex(wc => wc.IsActive);
    }
}
