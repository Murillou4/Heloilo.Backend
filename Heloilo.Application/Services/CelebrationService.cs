using Heloilo.Application.DTOs.Celebration;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class CelebrationService : ICelebrationService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<CelebrationService> _logger;

    public CelebrationService(HeloiloDbContext context, ILogger<CelebrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AnniversaryInfoDto> GetAnniversaryInfoAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        if (!relationship.RelationshipStartDate.HasValue)
        {
            return new AnniversaryInfoDto
            {
                DaysTogether = 0,
                RelationshipStartDate = null,
                NextAnniversary = null,
                DaysUntilNextAnniversary = 0,
                IsAnniversaryToday = false
            };
        }

        var startDate = relationship.RelationshipStartDate.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysTogether = today.DayNumber - startDate.DayNumber;

        DateOnly? nextAnniversary = null;
        int daysUntilNext = 0;
        bool isToday = false;

        if (relationship.CelebrationType == CelebrationType.Annual)
        {
            var currentYear = today.Year;
            var anniversaryThisYear = new DateOnly(currentYear, startDate.Month, startDate.Day);
            
            if (anniversaryThisYear >= today)
            {
                nextAnniversary = anniversaryThisYear;
                daysUntilNext = anniversaryThisYear.DayNumber - today.DayNumber;
                isToday = daysUntilNext == 0;
            }
            else
            {
                var anniversaryNextYear = new DateOnly(currentYear + 1, startDate.Month, startDate.Day);
                nextAnniversary = anniversaryNextYear;
                daysUntilNext = anniversaryNextYear.DayNumber - today.DayNumber;
            }
        }
        else // Monthly
        {
            var nextMonth = today.AddMonths(1);
            nextAnniversary = new DateOnly(nextMonth.Year, nextMonth.Month, startDate.Day);
            daysUntilNext = nextAnniversary.Value.DayNumber - today.DayNumber;
            isToday = daysUntilNext == 0;
        }

        return new AnniversaryInfoDto
        {
            DaysTogether = daysTogether,
            RelationshipStartDate = startDate,
            NextAnniversary = nextAnniversary,
            DaysUntilNextAnniversary = daysUntilNext,
            IsAnniversaryToday = isToday
        };
    }

    public async Task<List<object>> GetUpcomingCelebrationsAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var celebrations = new List<object>();

        if (relationship.RelationshipStartDate.HasValue)
        {
            var info = await GetAnniversaryInfoAsync(userId);
            if (info.NextAnniversary.HasValue && info.DaysUntilNextAnniversary <= 30)
            {
                celebrations.Add(new
                {
                    Type = relationship.CelebrationType == CelebrationType.Annual ? "Aniversário" : "Mêsversário",
                    Date = info.NextAnniversary.Value,
                    DaysUntil = info.DaysUntilNextAnniversary,
                    IsToday = info.IsAnniversaryToday
                });
            }
        }

        return celebrations;
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }
}

