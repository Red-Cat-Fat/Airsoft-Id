using MediatR;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class RegisterAirsofterCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Callsign { get; set; } = string.Empty;
}
