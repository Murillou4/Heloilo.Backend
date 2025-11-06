using API;

namespace Heloilo.WebAPI.Middlewares;

public class ContentTypeValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ContentTypeValidationMiddleware> _logger;
    private readonly HashSet<string> _allowedContentTypes;
    private readonly HashSet<string> _jsonEndpoints;
    private readonly HashSet<string> _multipartEndpoints;

    public ContentTypeValidationMiddleware(
        RequestDelegate next,
        ILogger<ContentTypeValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Content types permitidos
        _allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/json",
            "application/x-www-form-urlencoded",
            "multipart/form-data",
            "text/plain"
        };

        // Endpoints que devem receber JSON
        _jsonEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/auth/register",
            "/auth/login",
            "/auth/refresh",
            "/auth/logout",
            "/users/me",
            "/userstatus",
            "/relationships",
            "/wishes",
            "/memories",
            "/moodlogs",
            "/dailyactivities",
            "/notifications",
            "/storypages",
            "/celebration",
            "/dashboard",
            "/search"
        };

        // Endpoints que podem receber multipart (uploads)
        _multipartEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/wishes",
            "/memories",
            "/chat/messages",
            "/users/me"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Pular validação para GET, DELETE, OPTIONS e health checks
        if (context.Request.Method == "GET" ||
            context.Request.Method == "DELETE" ||
            context.Request.Method == "OPTIONS" ||
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/hubs") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Para requisições com body, validar Content-Type
        if (context.Request.ContentLength > 0 || context.Request.HasFormContentType)
        {
            var contentType = context.Request.ContentType;
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Verificar se endpoint requer JSON específico
            var requiresJson = _jsonEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
            var allowsMultipart = _multipartEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(contentType))
            {
                // Content-Type ausente para requisições com body
                if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
                {
                    _logger.LogWarning("Requisição sem Content-Type. Path: {Path}", path);
                    
                    var response = RouteMessages.BadRequest(
                        "Content-Type é obrigatório para esta requisição",
                        "Content-Type inválido"
                    );

                    context.Response.StatusCode = response.StatusCode ?? 400;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(response.Value);
                    return;
                }
            }
            else
            {
                var contentTypeLower = contentType.ToLowerInvariant();
                var mainContentType = contentTypeLower.Split(';')[0].Trim();

                // Validar Content-Type baseado no endpoint
                if (requiresJson && !mainContentType.Contains("application/json"))
                {
                    _logger.LogWarning(
                        "Content-Type inválido para endpoint JSON. Recebido: {ContentType}, Path: {Path}",
                        contentType,
                        path
                    );

                    var response = RouteMessages.BadRequest(
                        "Este endpoint requer Content-Type: application/json",
                        "Content-Type inválido"
                    );

                    context.Response.StatusCode = response.StatusCode ?? 400;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(response.Value);
                    return;
                }

                // Verificar se Content-Type é permitido
                var isAllowed = _allowedContentTypes.Any(allowed => 
                    mainContentType.Contains(allowed, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed && !allowsMultipart && !mainContentType.Contains("multipart/form-data"))
                {
                    _logger.LogWarning(
                        "Content-Type não permitido. Recebido: {ContentType}, Path: {Path}",
                        contentType,
                        path
                    );

                    var response = RouteMessages.BadRequest(
                        $"Content-Type não permitido. Tipos permitidos: {string.Join(", ", _allowedContentTypes)}",
                        "Content-Type inválido"
                    );

                    context.Response.StatusCode = response.StatusCode ?? 400;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(response.Value);
                    return;
                }
            }
        }

        await _next(context);
    }
}

