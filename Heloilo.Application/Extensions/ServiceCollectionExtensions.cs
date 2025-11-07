using Heloilo.Application.Interfaces;
using Heloilo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Heloilo.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();

        // User services
        services.AddScoped<IUserService, UserService>();

        // Relationship services
        services.AddScoped<IRelationshipService, RelationshipService>();

        // Wish services
        services.AddScoped<IWishService, WishService>();

        // Memory services
        services.AddScoped<IMemoryService, MemoryService>();

        // Mood services
        services.AddScoped<IMoodLogService, MoodLogService>();

        // Activity services
        services.AddScoped<IActivityService, ActivityService>();

        // Status services
        services.AddScoped<IUserStatusService, UserStatusService>();

        // Chat services
        services.AddScoped<IChatService, ChatService>();

        // Notification services
        services.AddScoped<INotificationService, NotificationService>();

        // Story services
        services.AddScoped<IStoryService, StoryService>();

        // Celebration services
        services.AddScoped<ICelebrationService, CelebrationService>();

        // Media services
        services.AddScoped<IMediaService, MediaService>();

        // Favorite services
        services.AddScoped<IFavoriteService, FavoriteService>();

        // Shared content services
        services.AddScoped<ISharedContentService, SharedContentService>();

        // Data export services
        services.AddScoped<IDataExportService, DataExportService>();

        // Analytics services
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Reminder services
        services.AddScoped<IReminderService, ReminderService>();

        // Backup services
        services.AddScoped<IBackupService, BackupService>();
        
        return services;
    }
}

