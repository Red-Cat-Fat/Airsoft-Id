using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Auth;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class RegisterAirsofterCommandHandler : IRequestHandler<RegisterAirsofterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<RegisterAirsofterCommandHandler> _logger;

    public RegisterAirsofterCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IIdentityService identityService,
        ILogger<RegisterAirsofterCommandHandler> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterAirsofterCommand request, CancellationToken cancellationToken)
    {
        var existingAirsofter = await _context.Airsofters
            .FirstOrDefaultAsync(a => a.Callsign == request.Callsign, cancellationToken);

        if (existingAirsofter != null)
        {
            throw new Common.Exceptions.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure("Callsign", "Callsign already exists") });
        }

        var (succeeded, errors) = await _identityService.CreateUserAsync(request.Email, request.Password);
        if (!succeeded)
        {
            throw new Common.Exceptions.ValidationException(
                errors.Select(e => new FluentValidation.Results.ValidationFailure("Identity", e)));
        }

        var userId = await _identityService.FindByEmailAsync(request.Email);
        if (userId == null)
        {
            throw new Common.Exceptions.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure("Identity", "Failed to create user") });
        }

        var (roleSucceeded, roleErrors) = await _identityService.AddToRoleAsync(userId.Value, "Airsofter");
        if (!roleSucceeded)
        {
            _logger.LogWarning("Failed to assign Airsofter role: {Errors}", string.Join(", ", roleErrors));
        }

        var airsofter = new Airsofter
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Callsign = request.Callsign,
            UserId = userId.Value
        };

        _context.Airsofters.Add(airsofter);
        await _context.SaveChangesAsync(cancellationToken);

        var authResponse = await _tokenService.GenerateTokensAsync(userId.Value, request.Email, request.Callsign);

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return authResponse;
    }
}
