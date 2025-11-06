using System.Net;
using Microsoft.IdentityModel.Tokens;
using API;

namespace Heloilo.WebAPI.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Se a resposta já foi iniciada, não podemos modificá-la
        if (context.Response.HasStarted)
        {
            return;
        }

        var response = exception switch
        {
            UnauthorizedAccessException => RouteMessages.Unauthorized(
                exception.Message,
                "Não autorizado"
            ),
            InvalidOperationException => RouteMessages.BadRequest(
                exception.Message,
                "Operação inválida"
            ),
            SecurityTokenException => RouteMessages.Unauthorized(
                exception.Message,
                "Token inválido"
            ),
            ArgumentException => RouteMessages.BadRequest(
                exception.Message,
                "Dados inválidos"
            ),
            KeyNotFoundException => RouteMessages.NotFound(
                exception.Message,
                "Recurso não encontrado"
            ),
            FileNotFoundException => RouteMessages.NotFound(
                exception.Message,
                "Arquivo não encontrado"
            ),
            _ => RouteMessages.InternalError(
                "Ocorreu um erro interno no servidor",
                "Erro interno",
                new Dictionary<string, object>
                {
                    { "error", exception.Message },
                    { "timestamp", DateTime.UtcNow }
                }
            )
        };

        context.Response.StatusCode = response.StatusCode ?? 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response.Value);
    }
}

