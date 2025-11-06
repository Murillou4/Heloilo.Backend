using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }
        return userId;
    }

    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    protected string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    protected ActionResult? ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Message = e.ErrorMessage }))
                .ToList();

            var errorMessages = errors.Select(e => $"{e.Field}: {e.Message}").ToList();
            var errorData = new Dictionary<string, object>
            {
                { "errors", errors },
                { "errorCount", errors.Count }
            };

            return RouteMessages.BadRequest(
                string.Join("; ", errorMessages),
                "Erro de validação",
                errorData
            );
        }
        return null;
    }
}

