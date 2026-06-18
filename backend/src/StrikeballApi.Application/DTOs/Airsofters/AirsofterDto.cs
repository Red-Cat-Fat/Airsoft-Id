namespace StrikeballApi.Application.DTOs.Airsofters;

public class AirsofterDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Callsign { get; set; } = string.Empty;
    public string? KitDescription { get; set; }
    public string? AirsoftGunsList { get; set; }
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int GamesPlayed { get; set; }
    public int ViolationsCount { get; set; }
}
