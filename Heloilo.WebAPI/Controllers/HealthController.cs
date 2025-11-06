using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Heloilo.Infrastructure.Data;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HeloiloDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Verificar algumas métricas básicas
            var userCount = canConnect ? await _context.Users.CountAsync() : 0;
            var activeRelationshipsCount = canConnect ? await _context.Relationships.CountAsync(r => r.IsActive && r.DeletedAt == null) : 0;
            
            var data = new Dictionary<string, object>
            {
                { "status", canConnect ? "healthy" : "unhealthy" },
                { "database", canConnect ? "connected" : "disconnected" },
                { "metrics", new Dictionary<string, object>
                    {
                        { "totalUsers", userCount },
                        { "activeRelationships", activeRelationshipsCount }
                    }
                },
                { "timestamp", DateTime.UtcNow },
                { "version", "1.0.0" }
            };
            return RouteMessages.Ok("Health check realizado", canConnect ? "Sistema saudável" : "Problemas detectados", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do sistema");
            var data = new Dictionary<string, object>
            {
                { "status", "unhealthy" },
                { "database", "error" },
                { "timestamp", DateTime.UtcNow }
            };
            return RouteMessages.InternalError("Erro ao verificar saúde do sistema", "Sistema não saudável", data);
        }
    }

    [HttpGet("ready")]
    public async Task<ActionResult> Readiness()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(503, new { status = "not ready", message = "Database não está disponível" });
            }
            return Ok(new { status = "ready", message = "Sistema pronto para receber requisições" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar readiness");
            return StatusCode(503, new { status = "not ready", message = "Erro ao verificar readiness" });
        }
    }

    [HttpGet("live")]
    public ActionResult Liveness()
    {
        return Ok(new { status = "alive", message = "Sistema está em execução" });
    }
}

