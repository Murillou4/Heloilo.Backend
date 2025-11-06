using Heloilo.Application.DTOs.User;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
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
            var photoBase64 = await _userService.UploadProfilePhotoAsync(userId, file);
            var data = new Dictionary<string, object> { { "photoBase64", photoBase64 ?? string.Empty } };
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

}


