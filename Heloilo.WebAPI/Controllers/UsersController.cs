using Heloilo.Application.DTOs.User;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;
using System.Text;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IDataExportService _dataExportService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IDataExportService dataExportService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _dataExportService = dataExportService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetCurrentUserAsync(userId);
            var data = new Dictionary<string, object> { { "user", user } };
            return RouteMessages.Ok("Usuário obtido com sucesso", "Usuário encontrado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário atual");
            return RouteMessages.InternalError("Erro ao obter usuário", "Erro interno");
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            var user = await _userService.UpdateUserAsync(userId, dto);
            var data = new Dictionary<string, object> { { "user", user } };
            return RouteMessages.Ok("Perfil atualizado com sucesso", "Perfil atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil");
            return RouteMessages.InternalError("Erro ao atualizar perfil", "Erro interno");
        }
    }

    [HttpPut("me/theme")]
    public async Task<ActionResult> UpdateTheme([FromBody] UpdateThemeDto dto)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            var user = await _userService.UpdateThemeAsync(userId, dto);
            var data = new Dictionary<string, object> { { "user", user } };
            return RouteMessages.Ok("Tema atualizado com sucesso", "Tema atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tema");
            return RouteMessages.InternalError("Erro ao atualizar tema", "Erro interno");
        }
    }

    [HttpPost("me/photo")]
    public async Task<ActionResult> UploadPhoto(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return RouteMessages.BadRequest("Arquivo não fornecido", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            await _userService.UploadProfilePhotoAsync(userId, file);
            var data = new Dictionary<string, object> { { "photoId", userId } };
            return RouteMessages.Ok("Foto enviada com sucesso", "Foto atualizada", data);
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Arquivo inválido");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload da foto");
            return RouteMessages.InternalError("Erro ao fazer upload da foto", "Erro interno");
        }
    }

    [HttpGet("me/photo")]
    public async Task<ActionResult> GetPhoto()
    {
        try
        {
            var userId = GetCurrentUserId();
            var photo = await _userService.GetProfilePhotoAsync(userId);

            if (photo == null || photo.Length == 0)
            {
                return RouteMessages.BadRequest("Foto não encontrada", "Recurso não encontrado");
            }

            return File(photo, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter foto");
            return RouteMessages.InternalError("Erro ao obter foto", "Erro interno");
        }
    }

    [HttpPut("me/password")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            await _userService.UpdatePasswordAsync(userId, dto);
            return RouteMessages.Ok("Senha atualizada com sucesso", "Senha atualizada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Senha incorreta");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Dados inválidos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar senha");
            return RouteMessages.InternalError("Erro ao atualizar senha", "Erro interno");
        }
    }

    [HttpGet("me/export")]
    public async Task<ActionResult> ExportData([FromQuery] string? format = "json")
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (format?.ToLower() == "pdf")
            {
                var pdfData = await _dataExportService.ExportUserDataAsPdfAsync(userId);
                return File(pdfData, "application/pdf", $"heloilo-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
            }
            else
            {
                var jsonData = await _dataExportService.ExportUserDataAsJsonAsync(userId);
                var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                return File(jsonBytes, "application/json", $"heloilo-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
            }
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar dados do usuário");
            return RouteMessages.InternalError("Erro ao exportar dados", "Erro interno");
        }
    }

    [HttpPost("me/delete-request")]
    public async Task<ActionResult> RequestAccountDeletion()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.RequestAccountDeletionAsync(userId);
            var data = new Dictionary<string, object>
            {
                { "message", "Solicitação de exclusão criada. Sua conta será excluída em 30 dias. Você pode cancelar a qualquer momento antes dessa data." },
                { "deletionScheduledAt", DateTime.UtcNow.AddDays(30) }
            };
            return RouteMessages.Ok("Solicitação de exclusão criada com sucesso", "Exclusão agendada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar exclusão de conta");
            return RouteMessages.InternalError("Erro ao solicitar exclusão", "Erro interno");
        }
    }

    [HttpPost("me/cancel-deletion")]
    public async Task<ActionResult> CancelAccountDeletion()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.CancelAccountDeletionAsync(userId);
            return RouteMessages.Ok("Exclusão de conta cancelada com sucesso", "Exclusão cancelada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar exclusão de conta");
            return RouteMessages.InternalError("Erro ao cancelar exclusão", "Erro interno");
        }
    }

    [HttpDelete("me")]
    public async Task<ActionResult> DeleteAccount()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.DeleteAccountAsync(userId);
            return RouteMessages.Ok("Conta excluída com sucesso", "Conta excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Usuário não encontrado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta");
            return RouteMessages.InternalError("Erro ao excluir conta", "Erro interno");
        }
    }

}


