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
    /// Valida e normaliza parâmetros de paginação com mensagens de erro mais descritivas
    /// </summary>
    /// <param name="page">Número da página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20)</param>
    /// <param name="defaultPageSize">Tamanho padrão da página (default: 20)</param>
    /// <param name="maxPageSize">Tamanho máximo da página (default: 100)</param>
    /// <returns>Tupla com (page, pageSize) validados e normalizados</returns>
    protected (int Page, int PageSize) ValidateAndNormalizePagination(int page = 1, int pageSize = 20, int defaultPageSize = 20, int maxPageSize = 100)
    {
        var originalPage = page;
        var originalPageSize = pageSize;

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = defaultPageSize;
        }

        if (pageSize > maxPageSize)
        {
            pageSize = maxPageSize;
        }

        return (page, pageSize);
    }

    /// <summary>
    /// Valida um range de datas
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>ActionResult com erro se inválido, null se válido</returns>
    protected ActionResult? ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        var (isValid, errorMessage) = FilterHelper.ValidateDateRange(startDate, endDate);
        if (!isValid)
        {
            return RouteMessages.BadRequest(errorMessage ?? "Erro na validação de datas", "Erro de validação");
        }
        return null;
    }

    /// <summary>
    /// Valida um filtro de status
    /// </summary>
    /// <typeparam name="T">Tipo do enum de status</typeparam>
    /// <param name="status">Status a validar</param>
    /// <param name="allowedStatuses">Lista de status permitidos</param>
    /// <returns>ActionResult com erro se inválido, null se válido</returns>
    protected ActionResult? ValidateStatusFilter<T>(T? status, IEnumerable<T> allowedStatuses) where T : struct, Enum
    {
        var (isValid, errorMessage) = FilterHelper.ValidateStatus(status, allowedStatuses);
        if (!isValid)
        {
            return RouteMessages.BadRequest(errorMessage ?? "Status inválido", "Erro de validação");
        }
        return null;
    }

    /// <summary>
    /// Valida múltiplos filtros combinados (AND/OR)
    /// </summary>
    /// <param name="filters">Dicionário de filtros (nome do filtro, valor)</param>
    /// <param name="logic">Lógica de combinação: 'AND' ou 'OR' (default: 'AND')</param>
    /// <returns>ActionResult com erro se inválido, null se válido</returns>
    protected ActionResult? ValidateCombinedFilters(Dictionary<string, object?> filters, string logic = "AND")
    {
        if (!FilterHelper.ValidateCombinedFilters(filters, logic))
        {
            return RouteMessages.BadRequest(
                $"Filtros inválidos com lógica {logic}. Verifique os parâmetros fornecidos.",
                "Erro de validação"
            );
        }
        return null;
    }
}

