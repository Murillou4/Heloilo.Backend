using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Heloilo.Infrastructure.Data;
using Heloilo.WebAPI.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

// Add SignalR
builder.Services.AddSignalR();

// Add Controllers
builder.Services.AddControllers();

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

builder.Services.AddAuthorization();

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Heloilo API",
        Version = "v1",
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
});

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HeloiloDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Heloilo API v1");
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
