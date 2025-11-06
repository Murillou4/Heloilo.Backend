using System.Text.RegularExpressions;

namespace Heloilo.Application.Helpers;

public static class InputSanitizationHelper
{
    // Padrões de caracteres perigosos
    private static readonly Regex HtmlTagsRegex = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ScriptTagsRegex = new(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex JavascriptRegex = new(@"javascript:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex VbscriptRegex = new(@"vbscript:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OnEventRegex = new(@"on\w+\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Sanitiza uma string removendo tags HTML e scripts potencialmente perigosos
    /// </summary>
    /// <param name="input">String de entrada</param>
    /// <param name="allowHtml">Se true, permite HTML básico (default: false)</param>
    /// <returns>String sanitizada</returns>
    public static string SanitizeString(string? input, bool allowHtml = false)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Remover scripts perigosos primeiro
        var sanitized = ScriptTagsRegex.Replace(input, string.Empty);
        sanitized = JavascriptRegex.Replace(sanitized, string.Empty);
        sanitized = VbscriptRegex.Replace(sanitized, string.Empty);
        sanitized = OnEventRegex.Replace(sanitized, string.Empty);

        if (!allowHtml)
        {
            // Remover todas as tags HTML
            sanitized = HtmlTagsRegex.Replace(sanitized, string.Empty);
        }

        // Escapar caracteres especiais HTML
        sanitized = System.Net.WebUtility.HtmlEncode(sanitized);

        return sanitized.Trim();
    }

    /// <summary>
    /// Sanitiza uma string mantendo quebras de linha e espaços
    /// </summary>
    public static string SanitizeStringPreserveFormatting(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var sanitized = SanitizeString(input, allowHtml: false);
        
        // Restaurar quebras de linha básicas (se necessário)
        sanitized = sanitized.Replace("&lt;br&gt;", "\n");
        sanitized = sanitized.Replace("&lt;br/&gt;", "\n");

        return sanitized;
    }

    /// <summary>
    /// Valida se uma string contém conteúdo potencialmente perigoso
    /// </summary>
    public static bool ContainsPotentiallyDangerousContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return ScriptTagsRegex.IsMatch(input) ||
               JavascriptRegex.IsMatch(input) ||
               VbscriptRegex.IsMatch(input) ||
               OnEventRegex.IsMatch(input);
    }

    /// <summary>
    /// Remove caracteres de controle e caracteres não imprimíveis
    /// </summary>
    public static string RemoveControlCharacters(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Normaliza espaços em branco (remove múltiplos espaços consecutivos)
    /// </summary>
    public static string NormalizeWhitespace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Sanitiza uma URL removendo caracteres perigosos mas mantendo a estrutura válida
    /// </summary>
    public static string SanitizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        // Remover scripts e javascript:
        var sanitized = JavascriptRegex.Replace(url, string.Empty);
        sanitized = VbscriptRegex.Replace(sanitized, string.Empty);
        sanitized = ScriptTagsRegex.Replace(sanitized, string.Empty);

        // Remover caracteres de controle
        sanitized = RemoveControlCharacters(sanitized);

        return sanitized.Trim();
    }
}

