using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class OrganizerFieldAuthorization : BaseEntity
{
    public bool CanPublishGames { get; set; }
    public bool CanModerateGames { get; set; }
    public bool CanRecordViolations { get; set; }
    public DateTime GrantedAt { get; set; }
    
    // Foreign keys
    public Guid OrganizerId { get; set; }
    public Guid FieldId { get; set; }
    
    // Navigation properties
    public Organizer Organizer { get; set; } = null!;
    public Field Field { get; set; } = null!;
}
