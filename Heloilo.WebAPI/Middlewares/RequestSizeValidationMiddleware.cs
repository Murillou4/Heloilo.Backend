using API;

namespace Heloilo.WebAPI.Middlewares;

public class RequestSizeValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestSizeValidationMiddleware> _logger;
    private readonly long _maxJsonRequestSize; // 10MB
    private readonly long _maxMultipartRequestSize; // 50MB

    public RequestSizeValidationMiddleware(
        RequestDelegate next,
        ILogger<RequestSizeValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        
        // Configurações do appsettings ou valores padrão
        var maxJsonSize = configuration.GetValue<long>("RequestLimits:MaxJsonSize", 10 * 1024 * 1024); // 10MB
        var maxMultipartSize = configuration.GetValue<long>("RequestLimits:MaxMultipartSize", 50 * 1024 * 1024); // 50MB
        
        _maxJsonRequestSize = maxJsonSize;
        _maxMultipartRequestSize = maxMultipartSize;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Pular validação para health checks, hubs SignalR e Swagger
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/hubs") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Verificar Content-Length
        if (context.Request.ContentLength.HasValue)
        {
            var contentLength = context.Request.ContentLength.Value;
            var contentType = context.Request.ContentType?.ToLowerInvariant() ?? "";

            // Determinar limite baseado no tipo de conteúdo
            long maxSize;
            if (contentType.Contains("multipart/form-data"))
            {
                maxSize = _maxMultipartRequestSize;
            }
            else if (contentType.Contains("application/json") || contentType.Contains("application/xml"))
            {
                maxSize = _maxJsonRequestSize;
            }
            else
            {
                // Para outros tipos, usar o limite menor
                maxSize = _maxJsonRequestSize;
            }

            if (contentLength > maxSize)
            {
                _logger.LogWarning(
                    "Requisição muito grande rejeitada. Tamanho: {Size} bytes, Limite: {Limit} bytes, Path: {Path}",
                    contentLength,
                    maxSize,
                    context.Request.Path
                );

                var response = RouteMessages.BadRequest(
                    $"Requisição muito grande. Tamanho máximo permitido: {maxSize / (1024 * 1024)}MB",
                    "Requisição muito grande"
                );

                context.Response.StatusCode = response.StatusCode ?? 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response.Value);
                return;
            }
        }

        await _next(context);
    }
}

