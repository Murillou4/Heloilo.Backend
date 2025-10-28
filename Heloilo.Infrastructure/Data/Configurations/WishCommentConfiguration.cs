using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class WishCommentConfiguration : IEntityTypeConfiguration<WishComment>
{
    public void Configure(EntityTypeBuilder<WishComment> builder)
    {
        builder.HasKey(wc => wc.Id);

        builder.Property(wc => wc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(wc => new { wc.WishId, wc.CreatedAt });

        // Relationships
        builder.HasOne(wc => wc.Wish)
            .WithMany(w => w.Comments)
            .HasForeignKey(wc => wc.WishId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wc => wc.User)
            .WithMany(u => u.WishComments)
            .HasForeignKey(wc => wc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


