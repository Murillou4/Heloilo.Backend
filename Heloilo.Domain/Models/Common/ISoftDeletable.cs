namespace Heloilo.Domain.Models.Common;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
