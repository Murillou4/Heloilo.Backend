using System.Collections.Concurrent;
using System.Net;

namespace Heloilo.WebAPI.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _maxRequests = 100; // 100 requisições
        _window = TimeSpan.FromMinutes(1); // por minuto
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Pular rate limiting para health checks e Swagger
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var key = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        _rateLimitStore.TryGetValue(key, out var rateLimitInfo);

        if (rateLimitInfo == null || now - rateLimitInfo.WindowStart > _window)
        {
            // Nova janela ou janela expirada
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 1,
                WindowStart = now
            };
            _rateLimitStore[key] = rateLimitInfo;
        }
        else
        {
            rateLimitInfo.RequestCount++;
        }

        // Verificar limite
        if (rateLimitInfo.RequestCount > _maxRequests)
        {
            _logger.LogWarning("Rate limit excedido para {Key}. Requisições: {Count}", key, rateLimitInfo.RequestCount);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "TOO_MANY_REQUESTS",
                message = "Muitas requisições. Tente novamente mais tarde.",
                title = "Limite de requisições excedido",
                status = 429,
                date = DateTime.UtcNow.ToString("o")
            }));
            return;
        }

        // Adicionar headers de rate limit
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _maxRequests - rateLimitInfo.RequestCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = rateLimitInfo.WindowStart.Add(_window).ToString("o");

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Priorizar IP do cliente
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Se autenticado, usar userId também para rate limiting por usuário
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return userId != null ? $"user:{userId}" : $"ip:{ip}";
    }

    private class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

