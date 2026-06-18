namespace StrikeballApi.Application.DTOs.Airsofters;

public class GameHistoryDto
{
    public Guid GameId { get; set; }
    public DateTime Date { get; set; }
    public string? FieldName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
