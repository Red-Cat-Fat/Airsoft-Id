using MediatR;
using Microsoft.Extensions.Logging;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ITokenService tokenService,
        IIdentityService identityService,
        ILogger<LoginCommandHandler> logger)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.FindByEmailAsync(request.Email);
        if (userId == null)
        {
            throw new Common.Exceptions.UnauthorizedException("Invalid email or password");
        }

        var passwordValid = await _identityService.CheckPasswordAsync(userId.Value, request.Password);
        if (!passwordValid)
        {
            throw new Common.Exceptions.UnauthorizedException("Invalid email or password");
        }

        var roles = await _identityService.GetRolesAsync(userId.Value);
        var result = await _tokenService.GenerateTokensAsync(userId.Value, request.Email, null);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        return result;
    }
}
