namespace Heloilo.Application.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(long userId);
    Task<List<BackupInfo>> ListBackupsAsync(long userId);
    Task<bool> RestoreFromBackupAsync(long userId, string backupId);
    Task<bool> DeleteBackupAsync(long userId, string backupId);
}

public class BackupInfo
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeInBytes { get; set; }
    public string Format { get; set; } = "json";
}

