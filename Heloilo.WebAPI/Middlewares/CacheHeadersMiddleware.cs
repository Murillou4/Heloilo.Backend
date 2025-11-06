namespace Heloilo.WebAPI.Middlewares;

public class CacheHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CacheHeadersMiddleware> _logger;

    public CacheHeadersMiddleware(RequestDelegate next, ILogger<CacheHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Só adicionar cache headers para requisições GET bem-sucedidas
        if (context.Request.Method == "GET" && context.Response.StatusCode == 200)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Endpoints que não devem ser cacheados
            var noCacheEndpoints = new[]
            {
                "/health",
                "/dashboard",
                "/userstatus/current",
                "/userstatus/partner",
                "/notifications",
                "/chat"
            };

            var shouldCache = !noCacheEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));

            if (shouldCache)
            {
                // Cache por 5 minutos para dados dinâmicos
                context.Response.Headers.Append("Cache-Control", "private, max-age=300");
                context.Response.Headers.Append("Vary", "Accept, Accept-Encoding, Authorization");
            }
            else
            {
                // Sem cache para dados sensíveis ou em tempo real
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                context.Response.Headers.Append("Pragma", "no-cache");
                context.Response.Headers.Append("Expires", "0");
            }

            // ETag para validação de cache (opcional - pode ser implementado mais tarde)
            // context.Response.Headers.Append("ETag", GenerateETag(context));
        }
        else if (context.Request.Method != "GET")
        {
            // Para métodos que modificam dados, garantir que não haja cache
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }
    }
}

