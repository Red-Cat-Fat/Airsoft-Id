using MediatR;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
        {
            throw new Common.Exceptions.UnauthorizedException("Invalid or expired refresh token");
        }

        return result;
    }
}
