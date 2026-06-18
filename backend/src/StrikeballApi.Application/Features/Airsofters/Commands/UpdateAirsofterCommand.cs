using MediatR;

namespace StrikeballApi.Application.Features.Airsofters.Commands;

public class UpdateAirsofterCommand : IRequest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? KitDescription { get; set; }
    public string? AirsoftGunsList { get; set; }
    public Guid? TeamId { get; set; }
}
