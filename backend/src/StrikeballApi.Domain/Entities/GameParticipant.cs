using StrikeballApi.Domain.Common;
using StrikeballApi.Domain.Enums;

namespace StrikeballApi.Domain.Entities;

public class GameParticipant : BaseEntity
{
    public DateTime RegisteredAt { get; set; }
    public ParticipantStatus Status { get; set; }
    
    // Foreign keys
    public Guid GameId { get; set; }
    public Guid AirsofterId { get; set; }
    public Guid? TeamId { get; set; }
    
    // Navigation properties
    public Game Game { get; set; } = null!;
    public Airsofter Airsofter { get; set; } = null!;
    public Team? Team { get; set; }
}
