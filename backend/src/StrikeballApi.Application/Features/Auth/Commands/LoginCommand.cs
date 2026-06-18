using MediatR;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
