using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Helpers;

public static class ValidationHelper
{
    // Constantes de tamanho de arquivo
    public const long MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10MB
    public const long MAX_VIDEO_SIZE = 50 * 1024 * 1024; // 50MB
    public const long MAX_AUDIO_SIZE = 20 * 1024 * 1024; // 20MB

    // MIME types permitidos
    private static readonly string[] ALLOWED_IMAGE_MIME_TYPES = new[]
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
    };

    private static readonly string[] ALLOWED_VIDEO_MIME_TYPES = new[]
    {
        "video/mp4", "video/quicktime", "video/x-msvideo"
    };

    private static readonly string[] ALLOWED_AUDIO_MIME_TYPES = new[]
    {
        "audio/mpeg", "audio/mp3", "audio/wav", "audio/ogg", "audio/webm"
    };

    public static bool IsValidDate(int day, int month, int year)
    {
        try
        {
            var date = new DateOnly(year, month, day);
            return date.Year >= 1900 && date <= DateOnly.FromDateTime(DateTime.Today);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true; // URL opcional
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static bool ValidateCharacterLimit(string? text, int maxLength)
    {
        if (text == null)
        {
            return true; // Texto opcional
        }

        return text.Length <= maxLength;
    }

    public static bool ValidateFileSize(long fileSize, long maxSizeInBytes)
    {
        return fileSize <= maxSizeInBytes;
    }

    public static (int Page, int PageSize) ValidatePagination(int page, int pageSize, int defaultPageSize = 20, int maxPageSize = 100)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = defaultPageSize;
        if (pageSize > maxPageSize) pageSize = maxPageSize;
        return (page, pageSize);
    }

    public static (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile? file, long maxSize = MAX_IMAGE_SIZE, int? maxWidth = null, int? maxHeight = null)
    {
        if (file == null || file.Length == 0)
        {
            return (true, string.Empty); // Arquivo opcional
        }

        if (file.Length > maxSize)
        {
            return (false, $"Imagem muito grande. Máximo permitido: {maxSize / (1024 * 1024)}MB");
        }

        if (!ALLOWED_IMAGE_MIME_TYPES.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, $"Tipo de arquivo não permitido. Use: JPEG, PNG, GIF ou WEBP");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            return (false, $"Extensão de arquivo não permitida. Use: {string.Join(", ", allowedExtensions)}");
        }

        // Validação de dimensões (opcional - requer biblioteca de imagem)
        // TODO: Implementar validação de dimensões se necessário usando biblioteca de imagem

        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateVideoFile(IFormFile? file, long maxSize = MAX_VIDEO_SIZE, int? maxDurationMinutes = null, string[]? allowedCodecs = null)
    {
        if (file == null || file.Length == 0)
        {
            return (true, string.Empty); // Arquivo opcional
        }

        if (file.Length > maxSize)
        {
            return (false, $"Vídeo muito grande. Máximo permitido: {maxSize / (1024 * 1024)}MB");
        }

        if (!ALLOWED_VIDEO_MIME_TYPES.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, $"Tipo de arquivo não permitido. Use: MP4, MOV ou AVI");
        }

        // Validação de codec e duração (opcional - requer biblioteca de vídeo)
        // TODO: Implementar validação de codec e duração se necessário

        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateAudioFile(IFormFile? file, long maxSize = MAX_AUDIO_SIZE, int? maxDurationMinutes = null)
    {
        if (file == null || file.Length == 0)
        {
            return (true, string.Empty); // Arquivo opcional
        }

        if (file.Length > maxSize)
        {
            return (false, $"Áudio muito grande. Máximo permitido: {maxSize / (1024 * 1024)}MB");
        }

        if (!ALLOWED_AUDIO_MIME_TYPES.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, $"Tipo de arquivo não permitido. Use: MP3, WAV, OGG ou WEBM");
        }

        // Validação de duração (opcional - requer biblioteca de áudio)
        // Limite padrão: 5 minutos conforme requisitos (RF69)
        if (maxDurationMinutes.HasValue)
        {
            // TODO: Implementar validação de duração do áudio usando biblioteca apropriada
            // Por enquanto, apenas valida tamanho como aproximação
        }

        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateMediaFile(IFormFile? file, bool allowVideo = false, bool allowAudio = false)
    {
        if (file == null || file.Length == 0)
        {
            return (true, string.Empty); // Arquivo opcional
        }

        // Verificar se é imagem
        var imageValidation = ValidateImageFile(file);
        if (imageValidation.IsValid)
        {
            return imageValidation;
        }

        // Verificar se é vídeo (se permitido)
        if (allowVideo)
        {
            var videoValidation = ValidateVideoFile(file);
            if (videoValidation.IsValid)
            {
                return videoValidation;
            }
        }

        // Verificar se é áudio (se permitido)
        if (allowAudio)
        {
            var audioValidation = ValidateAudioFile(file);
            if (audioValidation.IsValid)
            {
                return audioValidation;
            }
        }

        return (false, "Tipo de arquivo não permitido");
    }
}

