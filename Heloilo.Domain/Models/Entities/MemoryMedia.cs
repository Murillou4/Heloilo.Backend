using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class MemoryMedia : BaseEntity
{
    public long MemoryId { get; set; }

    public byte[] FileBlob { get; set; } = Array.Empty<byte>();

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string MimeType { get; set; } = string.Empty;

    // Navigation properties
    public virtual Memory Memory { get; set; } = null!;
}
