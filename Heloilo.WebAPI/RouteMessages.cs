using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API;

public static class RouteMessages
{
    private static readonly Dictionary<ResponseKind, (int Status, string Type, string DefaultMessage, string DefaultTitle)> ResponseMap =
        new()
        {
            { ResponseKind.Ok,            (200, "OK", "A requisição foi aceita e processada com sucesso.", "Operação realizada.") },
            { ResponseKind.BadRequest,    (400, "BAD_REQUEST", "Os dados informados são inválidos.", "Dados inválidos.") },
            { ResponseKind.Unauthorized,  (401, "UNAUTHORIZED", "Você não possui autorização.", "Não autorizado.") },
            { ResponseKind.Forbidden,     (403, "FORBIDDEN", "A requisição não pode ser completada.", "Requisição recusada.") },
            { ResponseKind.NotFound,      (404, "NOT_FOUND", "Recurso não encontrado.", "Não encontrado.") },
            { ResponseKind.InternalError, (500, "INTERNAL_SERVER_ERROR", "Erro genérico no servidor.", "Erro interno no servidor.") },
        };

    public static ObjectResult Build(
        ResponseKind kind,
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
    {
        var (status, type, defaultMsg, defaultTitle) = ResponseMap[kind];

        var response = new StandardResponse(
            Type: type,
            Message: message ?? defaultMsg,
            Title: title ?? defaultTitle,
            Status: status,
            Data: data,
            ExtendedResultCode: extendedResultCode ?? (status == 200 ? "#SUCCESS" : "#ERROR"),
            Date: DateTime.UtcNow.ToString("o")
        );

        return new ObjectResult(response) { StatusCode = status };
    }

    public static ObjectResult Ok(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.Ok, message, title, data, extendedResultCode);

    public static ObjectResult BadRequest(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.BadRequest, message, title, data, extendedResultCode);

    public static ObjectResult Unauthorized(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.Unauthorized, message, title, data, extendedResultCode);

    public static ObjectResult Forbidden(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.Forbidden, message, title, data, extendedResultCode);

    public static ObjectResult NotFound(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.NotFound, message, title, data, extendedResultCode);

    public static ObjectResult InternalError(
        string? message = null,
        string? title = null,
        Dictionary<string, object>? data = null,
        string? extendedResultCode = null)
        => Build(ResponseKind.InternalError, message, title, data, extendedResultCode);

    public static ObjectResult OkPaged<T>(
        string listName,
        Heloilo.Domain.Models.Common.PagedResult<T> paged,
        string? message = null,
        string? title = null,
        string? extendedResultCode = null)
    {
        var data = new Dictionary<string, object>
        {
            { listName, paged.Items },
            { nameof(paged.Page), paged.Page },
            { nameof(paged.PageSize), paged.PageSize },
            { nameof(paged.TotalItems), paged.TotalItems },
            { nameof(paged.TotalPages), paged.TotalPages },
            { nameof(paged.HasNextPage), paged.HasNextPage },
            { nameof(paged.HasPreviousPage), paged.HasPreviousPage },
            { nameof(paged.IsFirstPage), paged.IsFirstPage },
            { nameof(paged.IsLastPage), paged.IsLastPage },
        };

        return Ok(message, title, data, extendedResultCode);
    }
}

public enum ResponseKind
{
    Ok,
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
    InternalError
}

public record StandardResponse(
    string Type,
    string Message,
    string Title,
    int Status,
    Dictionary<string, object>? Data,
    string ExtendedResultCode,
    string Date
);

