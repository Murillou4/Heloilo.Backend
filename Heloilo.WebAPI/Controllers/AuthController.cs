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

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <param name="request">Dados de registro do usuário (email, senha, nome, apelido)</param>
    /// <returns>Dados do usuário criado e tokens de autenticação</returns>
    /// <response code="200">Usuário registrado com sucesso</response>
    /// <response code="400">Dados inválidos ou email já cadastrado</response>
    /// <response code="500">Erro interno do servidor</response>
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
                { "id", response.User.Id },
                { "email", response.User.Email },
                { "name", response.User.Name },
                { "nickname", response.User.Nickname ?? string.Empty },
                {"hasRelationship", response.HasRelationship},
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

    /// <summary>
    /// Autentica um usuário e retorna tokens de acesso
    /// </summary>
    /// <param name="request">Credenciais de login (email e senha)</param>
    /// <returns>Dados do usuário e tokens de autenticação</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="401">Credenciais inválidas</response>
    /// <response code="500">Erro interno do servidor</response>
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
                { "id", response.User.Id },
                { "email", response.User.Email },
                { "name", response.User.Name },
                { "nickname", response.User.Nickname ?? string.Empty },
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

    /// <summary>
    /// Renova o token de acesso usando um refresh token válido
    /// </summary>
    /// <param name="request">Refresh token atual</param>
    /// <returns>Novos tokens de autenticação</returns>
    /// <response code="200">Token renovado com sucesso</response>
    /// <response code="401">Refresh token inválido ou expirado</response>
    /// <response code="500">Erro interno do servidor</response>
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

    /// <summary>
    /// Solicita recuperação de senha
    /// </summary>
    /// <param name="request">Email do usuário</param>
    /// <returns>Confirmação de envio do email de recuperação</returns>
    /// <response code="200">Email de recuperação enviado (ou seria enviado em produção)</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            await _authService.ForgotPasswordAsync(request.Email);
            return RouteMessages.Ok(
                "Se o email estiver cadastrado, você receberá instruções para redefinir sua senha",
                "Solicitação processada"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar solicitação de recuperação de senha");
            return RouteMessages.InternalError("Erro ao processar solicitação", "Erro interno");
        }
    }

    /// <summary>
    /// Redefine a senha usando um token de recuperação
    /// </summary>
    /// <param name="request">Token e nova senha</param>
    /// <returns>Confirmação de redefinição de senha</returns>
    /// <response code="200">Senha redefinida com sucesso</response>
    /// <response code="400">Token inválido ou expirado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            return RouteMessages.Ok("Senha redefinida com sucesso", "Senha atualizada");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Erro ao redefinir senha");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao redefinir senha");
            return RouteMessages.InternalError("Erro ao processar redefinição de senha", "Erro interno");
        }
    }

    /// <summary>
    /// Verifica o email do usuário usando um token
    /// </summary>
    /// <param name="request">Token de verificação</param>
    /// <returns>Confirmação de verificação de email</returns>
    /// <response code="200">Email verificado com sucesso</response>
    /// <response code="400">Token inválido ou expirado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            await _authService.VerifyEmailAsync(request.Token);
            return RouteMessages.Ok("Email verificado com sucesso", "Verificação concluída");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Erro ao verificar email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar email");
            return RouteMessages.InternalError("Erro ao processar verificação de email", "Erro interno");
        }
    }

    /// <summary>
    /// Reenvia o email de verificação
    /// </summary>
    /// <param name="request">Email do usuário</param>
    /// <returns>Confirmação de reenvio do email</returns>
    /// <response code="200">Email de verificação reenviado (ou seria reenviado em produção)</response>
    /// <response code="400">Email já verificado ou dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            await _authService.ResendVerificationAsync(request.Email);
            return RouteMessages.Ok(
                "Se o email estiver cadastrado e não verificado, você receberá um novo email de verificação",
                "Solicitação processada"
            );
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Erro ao reenviar verificação");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar email de verificação");
            return RouteMessages.InternalError("Erro ao processar solicitação", "Erro interno");
        }
    }
}

