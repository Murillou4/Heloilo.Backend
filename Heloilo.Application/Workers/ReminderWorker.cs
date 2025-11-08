using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Workers;

public class ReminderWorker : BackgroundService
{
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderWorker> _logger;

    public ReminderWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderWorker iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar ciclo do ReminderWorker");
            }

            try
            {
                await Task.Delay(ExecutionInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ignorar cancelamento ao encerrar a aplicação
            }
        }
    }

    private async Task ProcessCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<HeloiloDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var nowUtc = DateTime.UtcNow;
        var windowEnd = nowUtc.AddHours(1);

        await ProcessRemindersAsync(context, notificationService, nowUtc, windowEnd, stoppingToken);
        await ProcessDailyActivitiesAsync(context, notificationService, nowUtc, windowEnd, stoppingToken);
        await ProcessCelebrationsAsync(context, notificationService, nowUtc, windowEnd, stoppingToken);
    }

    private async Task ProcessRemindersAsync(
        HeloiloDbContext context,
        INotificationService notificationService,
        DateTime nowUtc,
        DateTime windowEnd,
        CancellationToken cancellationToken)
    {
        var reminders = await context.Reminders
            .AsNoTracking()
            .Where(r => r.DeletedAt == null && r.IsActive && !r.IsCompleted && r.ReminderDate > nowUtc && r.ReminderDate <= windowEnd)
            .ToListAsync(cancellationToken);

        foreach (var reminder in reminders)
        {
            var relationshipId = await EnsureRelationshipIdAsync(context, reminder.UserId, reminder.RelationshipId, cancellationToken);
            if (!relationshipId.HasValue)
            {
                continue;
            }

            try
            {
                await notificationService.CreateAndSendNotificationAsync(
                    reminder.UserId,
                    relationshipId.Value,
                    "Lembrete em breve",
                    $"Não esqueça: {reminder.Title}",
                    NotificationType.Reminder);

                _logger.LogInformation("Notificação de lembrete enviada: ReminderId={ReminderId}, UserId={UserId}", reminder.Id, reminder.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao enviar notificação para o lembrete {ReminderId}", reminder.Id);
            }
        }
    }

    private async Task ProcessDailyActivitiesAsync(
        HeloiloDbContext context,
        INotificationService notificationService,
        DateTime nowUtc,
        DateTime windowEnd,
        CancellationToken cancellationToken)
    {
        var activities = await context.DailyActivities
            .AsNoTracking()
            .Where(a => !a.IsCompleted && a.ReminderMinutes != null && a.DeletedAt == null)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.Title,
                a.ActivityDate,
                a.ReminderMinutes
            })
            .ToListAsync(cancellationToken);

        foreach (var activity in activities)
        {
            var activityDateUtc = CreateUtcDateTime(activity.ActivityDate);
            var reminderTimeUtc = activityDateUtc.AddMinutes(-activity.ReminderMinutes!.Value);

            if (reminderTimeUtc < nowUtc || reminderTimeUtc > windowEnd)
            {
                continue;
            }

            var relationship = await RelationshipValidationHelper.ValidateUserRelationshipAsync(context, activity.UserId);
            if (relationship == null)
            {
                continue;
            }

            try
            {
                await notificationService.CreateAndSendNotificationAsync(
                    activity.UserId,
                    relationship.Id,
                    "Atividade se aproximando",
                    $"Sua atividade \"{activity.Title}\" está chegando.",
                    NotificationType.Activity);

                _logger.LogInformation("Notificação de atividade enviada: ActivityId={ActivityId}, UserId={UserId}", activity.Id, activity.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao enviar notificação para a atividade {ActivityId}", activity.Id);
            }
        }
    }

    private async Task ProcessCelebrationsAsync(
        HeloiloDbContext context,
        INotificationService notificationService,
        DateTime nowUtc,
        DateTime windowEnd,
        CancellationToken cancellationToken)
    {
        var relationships = await context.Relationships
            .AsNoTracking()
            .Where(r => r.IsActive && r.DeletedAt == null && r.RelationshipStartDate != null)
            .Select(r => new
            {
                r.Id,
                r.User1Id,
                r.User2Id,
                r.RelationshipStartDate,
                r.CelebrationType
            })
            .ToListAsync(cancellationToken);

        foreach (var relationship in relationships)
        {
            var nextCelebrationUtc = GetNextCelebrationUtc(relationship.RelationshipStartDate!.Value, relationship.CelebrationType);
            if (!nextCelebrationUtc.HasValue || nextCelebrationUtc < nowUtc || nextCelebrationUtc > windowEnd)
            {
                continue;
            }

            var celebrationName = relationship.CelebrationType == CelebrationType.Annual ? "aniversário" : "mêsversário";
            var title = $"Celebração do relacionamento";
            var content = $"Hoje é o {celebrationName} de vocês. Que tal planejar algo especial?";

            await SendAnniversaryNotificationAsync(notificationService, relationship.User1Id, relationship.Id, title, content);
            await SendAnniversaryNotificationAsync(notificationService, relationship.User2Id, relationship.Id, title, content);
        }
    }

    private static DateTime CreateUtcDateTime(DateOnly date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime? GetNextCelebrationUtc(DateOnly startDate, CelebrationType celebrationType)
    {
        if (startDate == default)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (celebrationType == CelebrationType.Annual)
        {
            var currentYear = today.Year;
            var targetDay = Math.Min(startDate.Day, DateTime.DaysInMonth(currentYear, startDate.Month));
            var anniversaryThisYear = new DateOnly(currentYear, startDate.Month, targetDay);

            if (anniversaryThisYear < today)
            {
                var nextYear = currentYear + 1;
                targetDay = Math.Min(startDate.Day, DateTime.DaysInMonth(nextYear, startDate.Month));
                anniversaryThisYear = new DateOnly(nextYear, startDate.Month, targetDay);
            }

            return CreateUtcDateTime(anniversaryThisYear);
        }
        else
        {
            var targetDay = Math.Min(startDate.Day, DateTime.DaysInMonth(today.Year, today.Month));
            var monthlyCelebration = new DateOnly(today.Year, today.Month, targetDay);

            if (monthlyCelebration < today)
            {
                var next = today.AddMonths(1);
                targetDay = Math.Min(startDate.Day, DateTime.DaysInMonth(next.Year, next.Month));
                monthlyCelebration = new DateOnly(next.Year, next.Month, targetDay);
            }

            return CreateUtcDateTime(monthlyCelebration);
        }
    }

    private async Task<long?> EnsureRelationshipIdAsync(
        HeloiloDbContext context,
        long userId,
        long? currentRelationshipId,
        CancellationToken cancellationToken)
    {
        if (currentRelationshipId.HasValue)
        {
            return currentRelationshipId;
        }

        var relationship = await RelationshipValidationHelper.ValidateUserRelationshipAsync(context, userId);
        return relationship?.Id;
    }

    private async Task SendAnniversaryNotificationAsync(
        INotificationService notificationService,
        long userId,
        long relationshipId,
        string title,
        string content)
    {
        try
        {
            await notificationService.CreateAndSendNotificationAsync(
                userId,
                relationshipId,
                title,
                content,
                NotificationType.Anniversary);

            _logger.LogInformation("Notificação de celebração enviada: RelationshipId={RelationshipId}, UserId={UserId}", relationshipId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar notificação de celebração para o usuário {UserId}", userId);
        }
    }
}

