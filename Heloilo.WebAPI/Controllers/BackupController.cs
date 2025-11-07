using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BackupController : BaseController
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateBackup()
    {
        try
        {
            var userId = GetCurrentUserId();
            var backupId = await _backupService.CreateBackupAsync(userId);
            var data = new Dictionary<string, object>
            {
                { "backupId", backupId },
                { "message", "Backup criado com sucesso" }
            };
            return RouteMessages.Ok("Backup criado com sucesso", "Backup criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar backup");
            return RouteMessages.InternalError("Erro ao criar backup", "Erro interno");
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult> ListBackups()
    {
        try
        {
            var userId = GetCurrentUserId();
            var backups = await _backupService.ListBackupsAsync(userId);
            var data = new Dictionary<string, object> { { "backups", backups } };
            return RouteMessages.Ok("Backups listados com sucesso", "Backups", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar backups");
            return RouteMessages.InternalError("Erro ao listar backups", "Erro interno");
        }
    }

    [HttpPost("restore")]
    public async Task<ActionResult> RestoreFromBackup([FromBody] RestoreBackupRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            await _backupService.RestoreFromBackupAsync(userId, request.BackupId);
            return RouteMessages.Ok("Backup restaurado com sucesso", "Backup restaurado");
        }
        catch (FileNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Backup não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao restaurar backup");
            return RouteMessages.InternalError("Erro ao restaurar backup", "Erro interno");
        }
    }

    [HttpDelete("{backupId}")]
    public async Task<ActionResult> DeleteBackup(string backupId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _backupService.DeleteBackupAsync(userId, backupId);
            return RouteMessages.Ok("Backup excluído com sucesso", "Backup excluído");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir backup");
            return RouteMessages.InternalError("Erro ao excluir backup", "Erro interno");
        }
    }
}

public class RestoreBackupRequest
{
    public string BackupId { get; set; } = string.Empty;
}

