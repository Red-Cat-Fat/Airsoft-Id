using MediatR;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}
