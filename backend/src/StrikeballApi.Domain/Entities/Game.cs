using StrikeballApi.Domain.Common;
using StrikeballApi.Domain.Enums;

namespace StrikeballApi.Domain.Entities;

public class Game : BaseEntity
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public GameStatus Status { get; set; }
    public string? CustomRules { get; set; }
    public bool UseFieldRules { get; set; }
    public int? MaxParticipants { get; set; }
    
    // Foreign keys
    public Guid FieldId { get; set; }
    public Guid OrganizerId { get; set; }
    
    // Navigation properties
    public Field Field { get; set; } = null!;
    public Organizer Organizer { get; set; } = null!;
    public ICollection<GameParticipant> Participants { get; set; } = new List<GameParticipant>();
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
