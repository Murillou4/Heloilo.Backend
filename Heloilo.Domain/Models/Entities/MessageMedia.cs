using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class MessageMedia : BaseEntity
{
    public long ChatMessageId { get; set; }

    public byte[] FileBlob { get; set; } = Array.Empty<byte>();

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string MimeType { get; set; } = string.Empty;

    // Navigation properties
    public virtual ChatMessage ChatMessage { get; set; } = null!;
}
