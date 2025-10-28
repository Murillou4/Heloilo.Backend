using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class MoodLogConfiguration : IEntityTypeConfiguration<MoodLog>
{
    public void Configure(EntityTypeBuilder<MoodLog> builder)
    {
        builder.HasKey(ml => ml.Id);

        builder.Property(ml => ml.Comment)
            .HasMaxLength(2000);

        builder.Property(ml => ml.LogDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(ml => new { ml.RelationshipId, ml.LogDate });
        builder.HasIndex(ml => new { ml.UserId, ml.LogDate });

        // Relationships
        builder.HasOne(ml => ml.User)
            .WithMany(u => u.MoodLogs)
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ml => ml.Relationship)
            .WithMany(r => r.MoodLogs)
            .HasForeignKey(ml => ml.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ml => ml.MoodType)
            .WithMany(mt => mt.MoodLogs)
            .HasForeignKey(ml => ml.MoodTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


