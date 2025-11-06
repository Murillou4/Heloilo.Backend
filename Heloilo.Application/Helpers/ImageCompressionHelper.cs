using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Helpers;

/// <summary>
/// Helper para compressão de imagens.
/// Nota: Para compressão real, adicione uma biblioteca como SixLabors.ImageSharp ou SkiaSharp.
/// Por enquanto, apenas valida e retorna o arquivo original se estiver abaixo do threshold.
/// </summary>
public static class ImageCompressionHelper
{
    private const long COMPRESSION_THRESHOLD = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// Comprime uma imagem se necessário.
    /// Atualmente apenas valida o tamanho - implementação real de compressão requer biblioteca externa.
    /// </summary>
    public static async Task<byte[]?> CompressImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        // Se a imagem já está abaixo do threshold, retorna sem compressão
        if (file.Length <= COMPRESSION_THRESHOLD)
        {
            using var stream1 = new MemoryStream();
            await file.CopyToAsync(stream1);
            return stream1.ToArray();
        }

        // TODO: Implementar compressão real usando biblioteca como SixLabors.ImageSharp
        // Por enquanto, retorna o arquivo original com aviso
        // Em produção, isso deve ser implementado para evitar uploads grandes
        using var stream2 = new MemoryStream();
        await file.CopyToAsync(stream2);
        return stream2.ToArray();
    }

    /// <summary>
    /// Comprime uma imagem de bytes se necessário.
    /// </summary>
    public static byte[]? CompressImage(byte[] imageBytes, string? mimeType = null)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return null;
        }

        // Se a imagem já está abaixo do threshold, retorna sem compressão
        if (imageBytes.Length <= COMPRESSION_THRESHOLD)
        {
            return imageBytes;
        }

        // TODO: Implementar compressão real
        // Por enquanto, retorna o arquivo original
        return imageBytes;
    }

    /// <summary>
    /// Verifica se uma imagem precisa de compressão.
    /// </summary>
    public static (bool NeedsCompression, long OriginalSize) CheckCompressionNeeded(long fileSize)
    {
        return (fileSize > COMPRESSION_THRESHOLD, fileSize);
    }

    /// <summary>
    /// Valida se o tamanho do arquivo está dentro dos limites após compressão.
    /// </summary>
    public static bool IsWithinSizeLimit(long fileSize, long maxSize = ValidationHelper.MAX_IMAGE_SIZE)
    {
        return fileSize <= maxSize;
    }
}

