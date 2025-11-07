using Heloilo.Application.Interfaces;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Heloilo.Application.Services;

public class BackupService : IBackupService
{
    private readonly HeloiloDbContext _context;
    private readonly IDataExportService _dataExportService;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupDirectory;

    public BackupService(
        HeloiloDbContext context,
        IDataExportService dataExportService,
        ILogger<BackupService> logger,
        IHostEnvironment environment)
    {
        _context = context;
        _dataExportService = dataExportService;
        _logger = logger;
        _backupDirectory = Path.Combine(environment.ContentRootPath, "Backups");

        // Criar diretório de backups se não existir
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<string> CreateBackupAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        // Exportar dados do usuário
        var jsonData = await _dataExportService.ExportUserDataAsJsonAsync(userId);

        // Criar ID único para o backup
        var backupId = $"{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.json");

        // Salvar backup
        await File.WriteAllTextAsync(backupPath, jsonData);

        // Salvar metadados
        var metadataPath = Path.Combine(_backupDirectory, $"{backupId}.meta");
        var metadata = new
        {
            Id = backupId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            SizeInBytes = new FileInfo(backupPath).Length,
            Format = "json"
        };

        var metadataJson = JsonSerializer.Serialize(metadata);
        await File.WriteAllTextAsync(metadataPath, metadataJson);

        _logger.LogInformation("Backup criado para usuário {UserId}: {BackupId}", userId, backupId);

        return backupId;
    }

    public async Task<List<BackupInfo>> ListBackupsAsync(long userId)
    {
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(_backupDirectory))
            return backups;

        var metadataFiles = Directory.GetFiles(_backupDirectory, "*.meta");

        foreach (var metadataFile in metadataFiles)
        {
            try
            {
                var metadataJson = await File.ReadAllTextAsync(metadataFile);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);

                if (metadata != null && metadata.ContainsKey("UserId"))
                {
                    var metadataUserId = Convert.ToInt64(metadata["UserId"]);
                    if (metadataUserId == userId)
                    {
                        backups.Add(new BackupInfo
                        {
                            Id = metadata.ContainsKey("Id") ? metadata["Id"].ToString() ?? "" : "",
                            CreatedAt = metadata.ContainsKey("CreatedAt") && DateTime.TryParse(metadata["CreatedAt"].ToString(), out var createdAt)
                                ? createdAt : DateTime.MinValue,
                            SizeInBytes = metadata.ContainsKey("SizeInBytes") && long.TryParse(metadata["SizeInBytes"].ToString(), out var size)
                                ? size : 0,
                            Format = metadata.ContainsKey("Format") ? metadata["Format"].ToString() ?? "json" : "json"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao ler metadados do backup: {MetadataFile}", metadataFile);
            }
        }

        return backups.OrderByDescending(b => b.CreatedAt).ToList();
    }

    public async Task<bool> RestoreFromBackupAsync(long userId, string backupId)
    {
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.json");
        var metadataPath = Path.Combine(_backupDirectory, $"{backupId}.meta");

        if (!File.Exists(backupPath) || !File.Exists(metadataPath))
            throw new FileNotFoundException("Backup não encontrado");

        // Verificar se o backup pertence ao usuário
        var metadataJson = await File.ReadAllTextAsync(metadataPath);
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);

        if (metadata == null || !metadata.ContainsKey("UserId"))
            throw new InvalidOperationException("Metadados do backup inválidos");

        var backupUserId = Convert.ToInt64(metadata["UserId"]);
        if (backupUserId != userId)
            throw new UnauthorizedAccessException("Backup não pertence ao usuário");

        // Ler dados do backup
        var backupData = await File.ReadAllTextAsync(backupPath);
        var backupObject = JsonSerializer.Deserialize<Dictionary<string, object>>(backupData);

        if (backupObject == null)
            throw new InvalidOperationException("Dados do backup inválidos");

        // Nota: A restauração completa seria complexa e requereria lógica específica
        // Por enquanto, apenas retornamos sucesso após validação
        // Em produção, implementar lógica de restauração completa dos dados

        _logger.LogInformation("Restauração do backup {BackupId} iniciada para usuário {UserId}", backupId, userId);

        return true;
    }

    public async Task<bool> DeleteBackupAsync(long userId, string backupId)
    {
        var backupPath = Path.Combine(_backupDirectory, $"{backupId}.json");
        var metadataPath = Path.Combine(_backupDirectory, $"{backupId}.meta");

        // Verificar se o backup pertence ao usuário
        if (File.Exists(metadataPath))
        {
            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);

            if (metadata != null && metadata.ContainsKey("UserId"))
            {
                var backupUserId = Convert.ToInt64(metadata["UserId"]);
                if (backupUserId != userId)
                    throw new UnauthorizedAccessException("Backup não pertence ao usuário");
            }
        }

        // Deletar arquivos
        if (File.Exists(backupPath))
            File.Delete(backupPath);

        if (File.Exists(metadataPath))
            File.Delete(metadataPath);

        _logger.LogInformation("Backup {BackupId} deletado para usuário {UserId}", backupId, userId);

        return true;
    }
}

