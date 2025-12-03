using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class Organizer : BaseEntity
{
    // Foreign key
    public Guid AirsofterId { get; set; }
    
    // Navigation properties
    public Airsofter Airsofter { get; set; } = null!;
    public ICollection<OrganizerFieldAuthorization> FieldAuthorizations { get; set; } = new List<OrganizerFieldAuthorization>();
    public ICollection<Game> OrganizedGames { get; set; } = new List<Game>();
}
