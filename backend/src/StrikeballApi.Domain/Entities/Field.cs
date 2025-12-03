using StrikeballApi.Domain.Common;

namespace StrikeballApi.Domain.Entities;

public class Field : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? MapUrl { get; set; }
    public string Rules { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Game> Games { get; set; } = new List<Game>();
    public ICollection<FieldBannedTeam> BannedTeams { get; set; } = new List<FieldBannedTeam>();
    public ICollection<OrganizerFieldAuthorization> OrganizerAuthorizations { get; set; } = new List<OrganizerFieldAuthorization>();
}
