using System.Diagnostics;

namespace Heloilo.WebAPI.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..16]; // ID curto para logs
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? requestId;
        
        // Adicionar correlation ID ao contexto
        context.Items["CorrelationId"] = correlationId;
        context.Items["RequestId"] = requestId;

        // Adicionar ao response header
        context.Response.Headers.Append("X-Request-Id", requestId);
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        var stopwatch = Stopwatch.StartNew();
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        // Log de início da requisição
        _logger.LogInformation(
            "Início da requisição. RequestId: {RequestId}, CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, UserId: {UserId}, IP: {IpAddress}",
            requestId,
            correlationId,
            context.Request.Method,
            context.Request.Path,
            userId,
            context.Connection.RemoteIpAddress?.ToString()
        );

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log de sucesso
            _logger.LogInformation(
                "Requisição concluída. RequestId: {RequestId}, CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms, UserId: {UserId}",
                requestId,
                correlationId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log de erro
            _logger.LogError(
                ex,
                "Erro na requisição. RequestId: {RequestId}, CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, Duration: {Duration}ms, UserId: {UserId}",
                requestId,
                correlationId,
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                userId
            );

            throw; // Re-throw para o GlobalExceptionHandlingMiddleware tratar
        }
    }
}

