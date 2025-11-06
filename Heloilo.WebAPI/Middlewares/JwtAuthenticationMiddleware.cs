using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Heloilo.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Heloilo.WebAPI.Middlewares;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var userId = authService.GetUserIdFromToken(token);
                if (!string.IsNullOrEmpty(userId))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    };

                    var identity = new ClaimsIdentity(claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // Token inválido, continuar sem autenticação
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}

