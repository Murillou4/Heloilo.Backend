using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Heloilo.Application.Helpers;
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

    /// <summary>
    /// Valida e normaliza parâmetros de paginação
    /// </summary>
    /// <param name="page">Número da página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20)</param>
    /// <param name="defaultPageSize">Tamanho padrão da página (default: 20)</param>
    /// <param name="maxPageSize">Tamanho máximo da página (default: 100)</param>
    /// <returns>Tupla com (page, pageSize) validados e normalizados</returns>
    protected (int Page, int PageSize) ValidateAndNormalizePagination(int page = 1, int pageSize = 20, int defaultPageSize = 20, int maxPageSize = 100)
    {
        return ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize, maxPageSize);
    }
}

