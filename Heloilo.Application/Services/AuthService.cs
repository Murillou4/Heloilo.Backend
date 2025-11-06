using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Heloilo.Application.DTOs.Auth;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace Heloilo.Application.Services;

public class AuthService : IAuthService
{
    private readonly HeloiloDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private const int MAX_LOGIN_ATTEMPTS = 5;
    private const int BLOCK_DURATION_MINUTES = 15;

    public AuthService(
        HeloiloDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar se email já existe
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.DeletedAt == null);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email já está em uso");
        }

        // Hash da senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Criar usuário
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Name = request.Name,
            Nickname = request.Nickname,
            ThemeColor = "#FF6B9D",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gerar tokens
        var (accessToken, refreshToken, expiresAt) = GenerateTokens(user);

        // Salvar refresh token (em produção, salvar em tabela separada)
        // Por enquanto, vamos retornar o token

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Nickname = user.Nickname,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Verificar bloqueio anti-brute force
        var blockKey = $"blocked:{request.Email.ToLowerInvariant()}";
        var attemptsKey = $"attempts:{request.Email.ToLowerInvariant()}";
        
        if (_cache.TryGetValue(blockKey, out DateTime blockedUntil))
        {
            if (blockedUntil > DateTime.UtcNow)
            {
                var remainingMinutes = (int)Math.Ceiling((blockedUntil - DateTime.UtcNow).TotalMinutes);
                _logger.LogWarning("Tentativa de login bloqueada para email: {Email}. Bloqueado até: {BlockedUntil}", 
                    request.Email, blockedUntil);
                throw new UnauthorizedAccessException($"Conta bloqueada. Tente novamente em {remainingMinutes} minuto(s).");
            }
            else
            {
                // Bloqueio expirado, limpar
                _cache.Remove(blockKey);
                _cache.Remove(attemptsKey);
            }
        }

        // Buscar usuário
        var user = await _context.Users
            .Include(u => u.RelationshipsAsUser1)
            .Include(u => u.RelationshipsAsUser2)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.DeletedAt == null);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Incrementar tentativas
            var attempts = _cache.GetOrCreate(attemptsKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(BLOCK_DURATION_MINUTES);
                return 0;
            });

            attempts++;
            _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(BLOCK_DURATION_MINUTES));

            // Bloquear após 5 tentativas
            if (attempts >= MAX_LOGIN_ATTEMPTS)
            {
                var blockUntil = DateTime.UtcNow.AddMinutes(BLOCK_DURATION_MINUTES);
                _cache.Set(blockKey, blockUntil, TimeSpan.FromMinutes(BLOCK_DURATION_MINUTES));
                _logger.LogWarning("Conta bloqueada por brute force para email: {Email}. Tentativas: {Attempts}", 
                    request.Email, attempts);
                throw new UnauthorizedAccessException($"Muitas tentativas de login. Conta bloqueada por {BLOCK_DURATION_MINUTES} minutos.");
            }

            _logger.LogWarning("Tentativa de login falhou para email: {Email}. Tentativa {Attempt}/{Max}", 
                request.Email, attempts, MAX_LOGIN_ATTEMPTS);
            throw new UnauthorizedAccessException("Email ou senha incorretos");
        }

        // Resetar tentativas em caso de sucesso
        _cache.Remove(blockKey);
        _cache.Remove(attemptsKey);

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Conta inativa");
        }

        // Verificar se tem relacionamento
        var hasRelationship = user.RelationshipsAsUser1.Any(r => r.IsActive && r.DeletedAt == null) ||
                             user.RelationshipsAsUser2.Any(r => r.IsActive && r.DeletedAt == null);

        // Gerar tokens
        var (accessToken, refreshToken, expiresAt) = GenerateTokens(user);

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Nickname = user.Nickname,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            HasRelationship = hasRelationship
        };
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var principal = GetPrincipalFromToken(refreshToken, isRefreshToken: true);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                throw new SecurityTokenException("Token inválido");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.DeletedAt != null || !user.IsActive)
            {
                throw new SecurityTokenException("Usuário não encontrado ou inativo");
            }

            var (accessToken, newRefreshToken, expiresAt) = GenerateTokens(user);

            return new RefreshTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao renovar token");
            throw new SecurityTokenException("Token de renovação inválido", ex);
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = GetPrincipalFromToken(token, isRefreshToken: false);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId);
            return user != null && user.DeletedAt == null && user.IsActive;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        // Em produção, implementar revogação de tokens em uma tabela
        // Por enquanto, apenas retornar true
        await Task.CompletedTask;
        return true;
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var principal = GetPrincipalFromToken(token, isRefreshToken: false);
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }

    private (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurado");
        var issuer = jwtSettings["Issuer"] ?? "Heloilo";
        var audience = jwtSettings["Audience"] ?? "Heloilo";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("nickname", user.Nickname ?? string.Empty)
        };

        // Access token - 7 dias (RNF30)
        var accessTokenExpiration = DateTime.UtcNow.AddDays(7);
        var accessToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: accessTokenExpiration,
            signingCredentials: credentials
        );

        // Refresh token - 30 dias (RNF33)
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(30);
        var refreshClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("type", "refresh")
        };

        var refreshToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: refreshClaims,
            expires: refreshTokenExpiration,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return (
            tokenHandler.WriteToken(accessToken),
            tokenHandler.WriteToken(refreshToken),
            accessTokenExpiration
        );
    }

    private ClaimsPrincipal GetPrincipalFromToken(string token, bool isRefreshToken)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurado");
        var issuer = jwtSettings["Issuer"] ?? "Heloilo";
        var audience = jwtSettings["Audience"] ?? "Heloilo";

        var tokenValidationParameters = new TokenValidationParameters
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

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token inválido");
        }

        // Verificar tipo de token
        if (isRefreshToken)
        {
            var tokenType = principal.FindFirst("type")?.Value;
            if (tokenType != "refresh")
            {
                throw new SecurityTokenException("Token não é um refresh token");
            }
        }

        return principal;
    }
}

