using System.Security.Claims;
using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Common.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> GenerateTokensAsync(Guid userId, string email, string? callsign);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    ClaimsPrincipal? ValidateToken(string token);
}
