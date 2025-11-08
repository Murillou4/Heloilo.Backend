using Heloilo.Application.DTOs.Shared;
using Heloilo.Application.Interfaces;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Heloilo.Application.Services;

public class MediaService : IMediaService
{
    private readonly HeloiloDbContext _context;

    public MediaService(HeloiloDbContext context)
    {
        _context = context;
    }

    public async Task<MediaFileResult?> GetUserPhotoAsync(long userId, long requestingUserId)
    {
        // Usuário pode ver sua própria foto ou a foto do parceiro
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Se não for o próprio usuário, verificar se é o parceiro
        if (userId != requestingUserId)
        {
            var relationship = await _context.Relationships
                .FirstOrDefaultAsync(r => 
                    ((r.User1Id == requestingUserId && r.User2Id == userId) ||
                     (r.User1Id == userId && r.User2Id == requestingUserId)) &&
                    r.IsActive && r.DeletedAt == null);

            if (relationship == null)
            {
                throw new UnauthorizedAccessException("Acesso negado");
            }
        }

        if (user.ProfilePhotoBlob == null || user.ProfilePhotoBlob.Length == 0)
        {
            return null;
        }

        return new MediaFileResult(user.ProfilePhotoBlob, "image/jpeg");
    }

    public async Task<MediaFileResult?> GetMemoryMediaAsync(long mediaId, long userId)
    {
        var media = await _context.MemoryMedia
            .Include(m => m.Memory)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new KeyNotFoundException("Mídia não encontrada");
        }

        if (media.Memory.DeletedAt != null)
        {
            throw new KeyNotFoundException("Memória não encontrada");
        }

        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);

        if (relationship == null || media.Memory.RelationshipId != relationship.Id)
        {
            throw new UnauthorizedAccessException("Acesso negado");
        }

        if (media.FileBlob == null || media.FileBlob.Length == 0)
        {
            return null;
        }

        var mimeType = string.IsNullOrWhiteSpace(media.MimeType) ? "application/octet-stream" : media.MimeType;
        return new MediaFileResult(media.FileBlob, mimeType);
    }

    public async Task<MediaFileResult?> GetMessageMediaAsync(long mediaId, long userId)
    {
        var media = await _context.MessageMedia
            .Include(m => m.ChatMessage)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new KeyNotFoundException("Mídia não encontrada");
        }

        if (media.ChatMessage.DeletedAt != null)
        {
            throw new KeyNotFoundException("Mensagem não encontrada");
        }

        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);

        if (relationship == null || media.ChatMessage.RelationshipId != relationship.Id)
        {
            throw new UnauthorizedAccessException("Acesso negado");
        }

        if (media.FileBlob == null || media.FileBlob.Length == 0)
        {
            return null;
        }

        var mimeType = string.IsNullOrWhiteSpace(media.MimeType) ? "application/octet-stream" : media.MimeType;
        return new MediaFileResult(media.FileBlob, mimeType);
    }

    public async Task<MediaFileResult?> GetStoryPageImageAsync(long pageId, long userId)
    {
        var page = await _context.StoryPages
            .FirstOrDefaultAsync(p => p.Id == pageId && p.DeletedAt == null);

        if (page == null)
        {
            throw new KeyNotFoundException("Página não encontrada");
        }

        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);

        if (relationship == null || page.RelationshipId != relationship.Id)
        {
            throw new UnauthorizedAccessException("Acesso negado");
        }

        if (page.ImageBlob == null || page.ImageBlob.Length == 0)
        {
            return null;
        }

        return new MediaFileResult(page.ImageBlob, "image/jpeg");
    }

    public async Task<MediaFileResult?> GetWishImageAsync(long wishId, long userId)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null)
        {
            throw new KeyNotFoundException("Desejo não encontrado");
        }

        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);

        if (relationship == null || wish.RelationshipId != relationship.Id)
        {
            throw new UnauthorizedAccessException("Acesso negado");
        }

        if (wish.ImageBlob == null || wish.ImageBlob.Length == 0)
        {
            return null;
        }

        return new MediaFileResult(wish.ImageBlob, "image/jpeg");
    }
}

