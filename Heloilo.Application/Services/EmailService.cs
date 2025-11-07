using Heloilo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string token, DateTime expiresAt)
    {
        // Em desenvolvimento, apenas logar
        // Em produção, implementar envio real de email via SMTP
        _logger.LogInformation(
            "Password reset email would be sent to {Email} with token {Token} (expires at {ExpiresAt}). " +
            "In development mode, token is returned in API response.",
            email, token, expiresAt);

        // TODO: Implementar envio real de email quando em produção
        // var smtpSettings = _configuration.GetSection("Smtp");
        // if (smtpSettings.Exists())
        // {
        //     // Implementar envio via SMTP
        // }

        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string token, DateTime expiresAt)
    {
        // Em desenvolvimento, apenas logar
        // Em produção, implementar envio real de email via SMTP
        _logger.LogInformation(
            "Verification email would be sent to {Email} with token {Token} (expires at {ExpiresAt}). " +
            "In development mode, token is returned in API response.",
            email, token, expiresAt);

        // TODO: Implementar envio real de email quando em produção
        // var smtpSettings = _configuration.GetSection("Smtp");
        // if (smtpSettings.Exists())
        // {
        //     // Implementar envio via SMTP
        // }

        await Task.CompletedTask;
        return true;
    }
}

