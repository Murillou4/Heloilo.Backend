namespace Heloilo.Application.DTOs.MoodLog;

public class MoodDashboardDto
{
    public Dictionary<string, int> MoodDistribution { get; set; } = new();
    public List<MoodLogDto> RecentLogs { get; set; } = new();
    public double AverageMoodScore { get; set; }
}

