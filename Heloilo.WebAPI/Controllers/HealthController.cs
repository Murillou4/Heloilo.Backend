using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Heloilo.Infrastructure.Data;
using System.Diagnostics;
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
            var stopwatch = Stopwatch.StartNew();
            var healthStatus = new Dictionary<string, object>();
            var isHealthy = true;

            // Verificar conexão com banco de dados
            var dbHealth = await CheckDatabaseHealthAsync();
            healthStatus["database"] = dbHealth;
            if (!dbHealth.ContainsKey("connected") || !(bool)dbHealth["connected"])
            {
                isHealthy = false;
            }

            // Verificar memória
            var memoryHealth = CheckMemoryHealth();
            healthStatus["memory"] = memoryHealth;

            // Verificar performance do banco
            var dbPerformance = await CheckDatabasePerformanceAsync();
            healthStatus["databasePerformance"] = dbPerformance;
            if (dbPerformance.ContainsKey("queryTimeMs") && (long)dbPerformance["queryTimeMs"] > 1000)
            {
                // Query muito lenta, mas não é crítico
                _logger.LogWarning("Query do health check levou {Time}ms", dbPerformance["queryTimeMs"]);
            }

            stopwatch.Stop();

            var metrics = new Dictionary<string, object>
            {
                { "totalUsers", dbHealth.ContainsKey("userCount") ? dbHealth["userCount"] : 0 },
                { "activeRelationships", dbHealth.ContainsKey("activeRelationships") ? dbHealth["activeRelationships"] : 0 },
                { "healthCheckDurationMs", stopwatch.ElapsedMilliseconds }
            };

            var data = new Dictionary<string, object>
            {
                { "status", isHealthy ? "healthy" : "unhealthy" },
                { "checks", healthStatus },
                { "metrics", metrics },
                { "timestamp", DateTime.UtcNow },
                { "version", "1.0.0" }
            };

            return RouteMessages.Ok(
                "Health check realizado",
                isHealthy ? "Sistema saudável" : "Problemas detectados",
                data
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do sistema");
            var data = new Dictionary<string, object>
            {
                { "status", "unhealthy" },
                { "error", ex.Message },
                { "timestamp", DateTime.UtcNow }
            };
            return RouteMessages.InternalError("Erro ao verificar saúde do sistema", "Sistema não saudável", data);
        }
    }

    private async Task<Dictionary<string, object>> CheckDatabaseHealthAsync()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var canConnect = await _context.Database.CanConnectAsync();
            stopwatch.Stop();

            if (canConnect)
            {
                var userCount = await _context.Users.CountAsync();
                var activeRelationshipsCount = await _context.Relationships
                    .CountAsync(r => r.IsActive && r.DeletedAt == null);

                return new Dictionary<string, object>
                {
                    { "connected", true },
                    { "connectionTimeMs", stopwatch.ElapsedMilliseconds },
                    { "userCount", userCount },
                    { "activeRelationships", activeRelationshipsCount }
                };
            }

            return new Dictionary<string, object>
            {
                { "connected", false },
                { "error", "Não foi possível conectar ao banco de dados" }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "connected", false },
                { "error", ex.Message }
            };
        }
    }

    private Dictionary<string, object> CheckMemoryHealth()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var gcMemory = GC.GetTotalMemory(false);

            // Limites de memória (pode ser configurável)
            var maxMemoryMB = 1024; // 1GB
            var memoryUsageMB = workingSet / (1024 * 1024);
            var isHealthy = memoryUsageMB < maxMemoryMB;

            return new Dictionary<string, object>
            {
                { "workingSetMB", memoryUsageMB },
                { "privateMemoryMB", privateMemory / (1024 * 1024) },
                { "gcMemoryMB", gcMemory / (1024 * 1024) },
                { "maxMemoryMB", maxMemoryMB },
                { "healthy", isHealthy },
                { "percentageUsed", (memoryUsageMB * 100) / maxMemoryMB }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "error", ex.Message },
                { "healthy", false }
            };
        }
    }

    private async Task<Dictionary<string, object>> CheckDatabasePerformanceAsync()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            // Executar uma query simples para medir tempo de resposta
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            stopwatch.Stop();

            return new Dictionary<string, object>
            {
                { "queryTimeMs", stopwatch.ElapsedMilliseconds },
                { "healthy", stopwatch.ElapsedMilliseconds < 500 }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "error", ex.Message },
                { "healthy", false }
            };
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

