using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class FieldBannedTeam : BaseEntity
{
    public string Reason { get; set; } = string.Empty;
    public DateTime BannedAt { get; set; }
    public DateTime? BannedUntil { get; set; }
    
    // Foreign keys
    public Guid FieldId { get; set; }
    public Guid TeamId { get; set; }
    
    // Navigation properties
    public Field Field { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
