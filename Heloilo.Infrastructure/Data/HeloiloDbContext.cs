using Microsoft.EntityFrameworkCore;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data;

public class HeloiloDbContext : DbContext
{
    public HeloiloDbContext(DbContextOptions<HeloiloDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Relationship> Relationships { get; set; } = null!;
    public DbSet<RelationshipInvitation> RelationshipInvitations { get; set; } = null!;
    public DbSet<InitialSetup> InitialSetups { get; set; } = null!;

    // Wish entities
    public DbSet<WishCategory> WishCategories { get; set; } = null!;
    public DbSet<Wish> Wishes { get; set; } = null!;
    public DbSet<WishComment> WishComments { get; set; } = null!;

    // Memory entities
    public DbSet<Memory> Memories { get; set; } = null!;
    public DbSet<MemoryMedia> MemoryMedia { get; set; } = null!;
    public DbSet<MemoryTag> MemoryTags { get; set; } = null!;

    // Mood entities
    public DbSet<MoodType> MoodTypes { get; set; } = null!;
    public DbSet<MoodLog> MoodLogs { get; set; } = null!;

    // Agenda and Status entities
    public DbSet<DailyActivity> DailyActivities { get; set; } = null!;
    public DbSet<UserStatus> UserStatuses { get; set; } = null!;

    // Chat entities
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<MessageMedia> MessageMedia { get; set; } = null!;

    // Notification entities
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    // Story entities
    public DbSet<StoryPage> StoryPages { get; set; } = null!;

    // Auth entities
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;

    // Favorite entities
    public DbSet<Favorite> Favorites { get; set; } = null!;

    // Shared content entities
    public DbSet<SharedContent> SharedContents { get; set; } = null!;

    // Reminder entities
    public DbSet<Reminder> Reminders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HeloiloDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Relationship>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Wish>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<WishComment>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Memory>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<DailyActivity>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<StoryPage>().HasQueryFilter(e => e.DeletedAt == null);

        // Global conventions for string properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                {
                    // Set default max length for unbounded strings
                    property.SetMaxLength(500);
                }
            }
        }

        // Seed data
        SeedWishCategories(modelBuilder);
        SeedMoodTypes(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void SeedWishCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WishCategory>().HasData(
            new WishCategory { Id = 1, Name = "Viagem", Emoji = "🌍", Description = "Lugares que o casal quer conhecer juntos", IsActive = true },
            new WishCategory { Id = 2, Name = "Compras / Presentes", Emoji = "🎁", Description = "Coisas que um quer ganhar ou comprar", IsActive = true },
            new WishCategory { Id = 3, Name = "Experiências", Emoji = "✨", Description = "Atividades e momentos a dois", IsActive = true },
            new WishCategory { Id = 4, Name = "Metas do Casal", Emoji = "🎯", Description = "Objetivos compartilhados", IsActive = true },
            new WishCategory { Id = 5, Name = "Casa e Decoração", Emoji = "🏡", Description = "Ideias para o lar", IsActive = true },
            new WishCategory { Id = 6, Name = "Datas Especiais", Emoji = "📅", Description = "Planos para aniversários e comemorações", IsActive = true },
            new WishCategory { Id = 7, Name = "Auto-cuidado", Emoji = "🧘‍♀️", Description = "Coisas individuais que melhoram o bem-estar", IsActive = true },
            new WishCategory { Id = 8, Name = "Animais de Estimação", Emoji = "🐾", Description = "Desejos relacionados a pets", IsActive = true },
            new WishCategory { Id = 9, Name = "Projetos Criativos", Emoji = "🎨", Description = "Sonhos artísticos ou hobbies", IsActive = true },
            new WishCategory { Id = 10, Name = "Gastronomia", Emoji = "🍝", Description = "Lugares para comer e receitas", IsActive = true },
            new WishCategory { Id = 11, Name = "Sonhos Grandes", Emoji = "🌠", Description = "Coisas mais distantes ou inspiracionais", IsActive = true },
            new WishCategory { Id = 12, Name = "Doações e Impacto", Emoji = "💗", Description = "Desejos voltados a ajudar outros", IsActive = true }
        );
    }

    private static void SeedMoodTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MoodType>().HasData(
            // Positivos
            new MoodType { Id = 1, Name = "Feliz / Contente", Emoji = "😊", MoodCategory = MoodCategory.Positive, Description = "Sensação de leveza ou satisfação", IsActive = true },
            new MoodType { Id = 2, Name = "Animado / Motivado", Emoji = "🚀", MoodCategory = MoodCategory.Positive, Description = "Energia para fazer coisas", IsActive = true },
            new MoodType { Id = 3, Name = "Calmo / Relaxado", Emoji = "😌", MoodCategory = MoodCategory.Positive, Description = "Paz interior, sem estresse", IsActive = true },
            new MoodType { Id = 4, Name = "Orgulhoso", Emoji = "😎", MoodCategory = MoodCategory.Positive, Description = "Quando sente que fez algo legal", IsActive = true },
            new MoodType { Id = 5, Name = "Grato / Satisfeito", Emoji = "🙏", MoodCategory = MoodCategory.Positive, Description = "Aprecia o que tem", IsActive = true },

            // Negativos
            new MoodType { Id = 6, Name = "Triste / Melancólico", Emoji = "😢", MoodCategory = MoodCategory.Negative, Description = "Desânimo ou sofrimento emocional", IsActive = true },
            new MoodType { Id = 7, Name = "Irritado / Frustrado", Emoji = "😠", MoodCategory = MoodCategory.Negative, Description = "Raiva ou impaciência", IsActive = true },
            new MoodType { Id = 8, Name = "Ansioso / Preocupado", Emoji = "😰", MoodCategory = MoodCategory.Negative, Description = "Sensação de tensão ou medo", IsActive = true },
            new MoodType { Id = 9, Name = "Culpado / Arrependido", Emoji = "😔", MoodCategory = MoodCategory.Negative, Description = "Pensamentos sobre erros", IsActive = true },
            new MoodType { Id = 10, Name = "Cansado / Desmotivado", Emoji = "😴", MoodCategory = MoodCategory.Negative, Description = "Falta de energia", IsActive = true },

            // Neutros
            new MoodType { Id = 11, Name = "Entediado", Emoji = "😐", MoodCategory = MoodCategory.Neutral, Description = "Sem estímulo ou interesse", IsActive = true },
            new MoodType { Id = 12, Name = "Confuso / Indeciso", Emoji = "🤔", MoodCategory = MoodCategory.Neutral, Description = "Não sabe bem o que pensar", IsActive = true },
            new MoodType { Id = 13, Name = "Curioso / Intrigado", Emoji = "🤨", MoodCategory = MoodCategory.Neutral, Description = "Mente ativa, querendo descobrir", IsActive = true }
        );
    }
}
