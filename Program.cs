using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Heloilo.Infrastructure.Data;
using Heloilo.WebAPI.Middlewares;
using Heloilo.WebAPI.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Heloilo.Application.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<HeloiloDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Memory Cache for brute force protection
builder.Services.AddMemoryCache();

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/plain", "text/css", "text/javascript" }
    );
});

// Configure compression levels
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Add Application services
Heloilo.Application.Extensions.ServiceCollectionExtensions.AddApplicationServices(builder.Services);

builder.Services.AddHostedService<ReminderWorker>();

// Add SignalR
builder.Services.AddSignalR();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    // ApiVersionReader padrão já suporta query string e header
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialização JSON para DateOnly
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // DateOnly é suportado nativamente no .NET 9, mas garantimos compatibilidade
    });

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurado");
var issuer = jwtSettings["Issuer"] ?? "Heloilo";
var audience = jwtSettings["Audience"] ?? "Heloilo";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Configurar eventos para SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Permitir acesso anônimo aos endpoints do Swagger
    options.FallbackPolicy = null; // Não requer autenticação por padrão, apenas onde [Authorize] estiver
});

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowedMethods = builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>() 
    ?? new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
var allowedHeaders = builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>()
    ?? new[] { "Content-Type", "Authorization", "X-Requested-With" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            // Produção: Origens específicas
            policy.WithOrigins(allowedOrigins)
                  .WithMethods(allowedMethods)
                  .WithHeaders(allowedHeaders)
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight requests
        }
        else
        {
            // Desenvolvimento: Mais permissivo mas ainda com algumas restrições
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders)
                      .SetPreflightMaxAge(TimeSpan.FromHours(1));
            }
            else
            {
                // Em produção sem configuração, usar política restritiva
                policy.WithOrigins("https://localhost") // Placeholder - deve ser configurado
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders)
                      .AllowCredentials();
            }
        }
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Função auxiliar local para gerar IDs de schema de forma segura
static string GetSafeSchemaId(Type type)
{
    if (type == null)
        return "Unknown";
    
    // Se FullName não for null, usar diretamente
    if (!string.IsNullOrEmpty(type.FullName))
        return type.FullName.Replace("+", ".").Replace("`", "");
    
    // Fallback: usar namespace + nome do tipo
    var namespacePrefix = !string.IsNullOrEmpty(type.Namespace) ? $"{type.Namespace}." : "";
    var typeName = type.Name.Replace("+", ".").Replace("`", "");
    
    // Para tipos genéricos, incluir informações dos tipos genéricos
    if (type.IsGenericType)
    {
        var genericArgs = type.GetGenericArguments()
            .Select(arg => GetSafeSchemaId(arg))
            .ToArray();
        var baseName = typeName.Split('`')[0];
        return $"{namespacePrefix}{baseName}[{string.Join(",", genericArgs)}]";
    }
    
    return $"{namespacePrefix}{typeName}";
}

builder.Services.AddSwaggerGen(c =>
{
    // Configurar para suportar múltiplas versões usando factory
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Heloilo API",
        Version = "1.0",
        Description = "API RESTful para o sistema Heloilo - Aplicativo para casais. " +
                      "Documentação completa dos endpoints, autenticação JWT e exemplos de uso.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Heloilo Team"
        }
    });

    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\". " +
                      "Para obter um token, use os endpoints /Auth/register ou /Auth/login",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentários XML se disponíveis
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Ignorar erros de esquema do Swagger para evitar problemas na geração
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    
    // Resolver conflitos de esquema usando o nome completo do tipo
    // Tratar casos onde FullName pode ser null (tipos genéricos, anônimos, etc.)
    c.CustomSchemaIds(type => GetSafeSchemaId(type));
    
    // Resolver conflitos de ações (se houver múltiplos endpoints com a mesma rota)
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // Configurar DateOnly como string no Swagger
    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
    
    c.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Nullable = true
    });
    
    // Configurar IFormFile como binary type para Swagger
    // Isso previne erros durante a geração de parâmetros antes dos filters processarem
    // Mapear apenas IFormFile uma vez - Swashbuckle trata IFormFile e IFormFile? como o mesmo tipo
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary",
        Description = "File upload"
    });
    
    // Mapear Dictionary<string, object> como object genérico para evitar problemas de schema
    c.MapType<Dictionary<string, object>>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        AdditionalProperties = new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "object",
            Description = "Dynamic value"
        },
        Description = "Dictionary with string keys and object values"
    });
    
    // Configurar suporte para form data e file uploads
    // IMPORTANTE: ParameterFilter deve vir ANTES do OperationFilter
    // para configurar os schemas antes que o Swagger tente processá-los
    // O ParameterFilter configura os schemas para evitar erros durante a geração
    c.ParameterFilter<FormFileParameterFilter>();
    // O OperationFilter remove os parâmetros do form e move para request body
    c.OperationFilter<FormFileOperationFilter>();
    
    
    // Adicionar filtro de schema para tratar tipos problemáticos e adicionar tratamento de erros
    c.SchemaFilter<SafeSchemaFilter>();
    
    // Configurar tratamento de erros na geração do Swagger
    c.CustomOperationIds(apiDesc =>
    {
        try
        {
            var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                var controllerName = actionDescriptor.ControllerName;
                var actionName = actionDescriptor.ActionName;
                return $"{controllerName}_{actionName}";
            }
            return apiDesc.RelativePath?.Replace("/", "_").Replace("{", "").Replace("}", "");
        }
        catch
        {
            return null;
        }
    });
});

var app = builder.Build();

// Apply migrations on startup or create database if migrations don't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HeloiloDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Try to apply migrations first
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", 
                pendingMigrations.Count, string.Join(", ", pendingMigrations));
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            // Check if migrations exist at all
            var migrations = db.Database.GetMigrations().ToList();
            if (migrations.Any())
            {
                logger.LogInformation("No pending migrations. Database is up to date.");
                // Verify tables exist
                try
                {
                    _ = db.Users.Count();
                }
                catch
                {
                    logger.LogWarning("Migrations applied but tables don't exist. Applying migrations again...");
                    db.Database.Migrate();
                }
            }
            else
            {
                // No migrations exist, use EnsureCreated
                logger.LogWarning("No migrations found. Creating database using EnsureCreated()...");
                EnsureDatabaseWithTables(db, logger);
            }
        }
    }
    catch (Exception ex)
    {
        // If migrations fail or don't exist, fallback to EnsureCreated
        logger.LogWarning(ex, "Error checking/applying migrations: {Message}. Creating database using EnsureCreated()...", ex.Message);
        EnsureDatabaseWithTables(db, logger);
    }
}

// Helper method to ensure database and tables exist
static void EnsureDatabaseWithTables(HeloiloDbContext db, ILogger logger)
{
    try
    {
        var created = db.Database.EnsureCreated();
        if (created)
        {
            logger.LogInformation("Database and tables created successfully using EnsureCreated()");
        }
        else
        {
            // Database exists, check if tables exist
            var canConnect = db.Database.CanConnect();
            if (canConnect)
            {
                // Try to query a table to see if schema exists
                try
                {
                    _ = db.Users.Count();
                    logger.LogInformation("Database already exists with schema (Users table accessible)");
                }
                catch
                {
                    // Tables don't exist even though database does - force recreation
                    logger.LogWarning("Database exists but tables are missing. Recreating schema...");
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    logger.LogInformation("Database schema recreated successfully");
                }
            }
            else
            {
                logger.LogInformation("Database connection check returned false, attempting to create...");
                db.Database.EnsureCreated();
            }
        }
    }
    catch (Exception ensureEx)
    {
        logger.LogError(ensureEx, "Failed to create database. Application may not function correctly.");
        throw;
    }
}

// Configure the HTTP request pipeline.
// Swagger deve vir antes de qualquer middleware de autenticação/autorização
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Heloilo API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Response Compression
app.UseResponseCompression();

// Security Headers (primeiro para proteger todas as respostas)
app.UseMiddleware<Heloilo.WebAPI.Middlewares.SecurityHeadersMiddleware>();

// Logging (para registrar todas as requisições)
app.UseMiddleware<Heloilo.WebAPI.Middlewares.LoggingMiddleware>();

app.UseCors();

// Request Size Validation (antes de processar o body)
app.UseMiddleware<Heloilo.WebAPI.Middlewares.RequestSizeValidationMiddleware>();

// Content Type Validation
app.UseMiddleware<Heloilo.WebAPI.Middlewares.ContentTypeValidationMiddleware>();

// Rate limiting
app.UseMiddleware<RateLimitingMiddleware>();

// Global exception handling
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Health checks endpoints são gerenciados pelo HealthController

// Custom JWT middleware for setting user context (deve vir antes de UseAuthentication)
app.UseMiddleware<JwtAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Cache Headers (depois da autenticação, antes dos controllers)
app.UseMiddleware<Heloilo.WebAPI.Middlewares.CacheHeadersMiddleware>();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<Heloilo.Application.Hubs.ChatHub>("/hubs/chat");
app.MapHub<Heloilo.Application.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
