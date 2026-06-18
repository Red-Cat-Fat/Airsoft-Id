namespace StrikeballApi.Application.DTOs.Airsofters;

public class ViolationDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? FieldName { get; set; }
}
