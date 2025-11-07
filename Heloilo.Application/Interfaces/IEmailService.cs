namespace Heloilo.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envia email de recuperação de senha
    /// </summary>
    /// <param name="email">Email do destinatário</param>
    /// <param name="token">Token de recuperação</param>
    /// <param name="expiresAt">Data de expiração do token</param>
    /// <returns>True se enviado com sucesso (ou em modo desenvolvimento, retorna true sempre)</returns>
    Task<bool> SendPasswordResetEmailAsync(string email, string token, DateTime expiresAt);

    /// <summary>
    /// Envia email de verificação
    /// </summary>
    /// <param name="email">Email do destinatário</param>
    /// <param name="token">Token de verificação</param>
    /// <param name="expiresAt">Data de expiração do token</param>
    /// <returns>True se enviado com sucesso (ou em modo desenvolvimento, retorna true sempre)</returns>
    Task<bool> SendVerificationEmailAsync(string email, string token, DateTime expiresAt);
}

