using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class Airsofter : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Callsign { get; set; } = string.Empty;
    public string? KitDescription { get; set; }
    public string? AirsoftGunsList { get; set; }
    
    // Foreign key
    public Guid? TeamId { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation properties
    public Team? Team { get; set; }
    public ICollection<GameParticipant> GameParticipants { get; set; } = new List<GameParticipant>();
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    public Organizer? Organizer { get; set; }
}
