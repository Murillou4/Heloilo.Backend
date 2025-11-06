using Heloilo.Application.DTOs.Auth;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var response = await _authService.RegisterAsync(request);
            var user = new Dictionary<string, object>
            {
                { "id", response.UserId },
                { "email", response.Email },
                { "name", response.Name },
                { "nickname", response.Nickname ?? string.Empty }
            };
            var data = new Dictionary<string, object>
            {
                { "user", user },
                { "token", response.AccessToken },
                { "refreshToken", response.RefreshToken },
                { "expiresAt", response.ExpiresAt }
            };
            return RouteMessages.Ok("Usuário registrado com sucesso", "Registro realizado", data);
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Erro ao registrar");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário");
            return RouteMessages.InternalError("Erro ao processar registro", "Erro interno");
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var response = await _authService.LoginAsync(request);
            var user = new Dictionary<string, object>
            {
                { "id", response.UserId },
                { "email", response.Email },
                { "name", response.Name },
                { "nickname", response.Nickname ?? string.Empty },
                { "hasRelationship", response.HasRelationship }
            };
            var data = new Dictionary<string, object>
            {
                { "user", user },
                { "token", response.AccessToken },
                { "refreshToken", response.RefreshToken },
                { "expiresAt", response.ExpiresAt }
            };
            return RouteMessages.Ok("Login realizado com sucesso", "Autenticação bem-sucedida", data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Credenciais inválidas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            return RouteMessages.InternalError("Erro ao processar login", "Erro interno");
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            var data = new Dictionary<string, object>
            {
                { "token", response.AccessToken },
                { "refreshToken", response.RefreshToken },
                { "expiresAt", response.ExpiresAt }
            };
            return RouteMessages.Ok("Token renovado com sucesso", "Token atualizado", data);
        }
        catch (SecurityTokenException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Token inválido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return RouteMessages.InternalError("Erro ao processar renovação de token", "Erro interno");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest? request)
    {
        try
        {
            if (request != null && !string.IsNullOrEmpty(request.RefreshToken))
            {
                await _authService.RevokeTokenAsync(request.RefreshToken);
            }

            return RouteMessages.Ok("Logout realizado com sucesso", "Sessão encerrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer logout");
            return RouteMessages.InternalError("Erro ao processar logout", "Erro interno");
        }
    }
}

