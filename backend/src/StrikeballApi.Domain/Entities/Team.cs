using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class Team : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string? LogoUrl { get; set; }
    
    // Navigation properties
    public ICollection<Airsofter> Members { get; set; } = new List<Airsofter>();
    public ICollection<GameParticipant> GameParticipants { get; set; } = new List<GameParticipant>();
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    public ICollection<FieldBannedTeam> BannedFromFields { get; set; } = new List<FieldBannedTeam>();
}
