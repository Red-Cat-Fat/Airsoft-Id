using StrikeballApi.Domain.Common;
using StrikeballApi.Domain.Enums;

namespace StrikeballApi.Domain.Entities;

public class Violation : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public ViolationSeverity Severity { get; set; }
    
    // Foreign keys
    public Guid? AirsofterId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid FieldId { get; set; }
    public Guid? GameId { get; set; }
    public Guid RecordedById { get; set; }
    
    // Navigation properties
    public Airsofter? Airsofter { get; set; }
    public Team? Team { get; set; }
    public Field Field { get; set; } = null!;
    public Game? Game { get; set; }
}
