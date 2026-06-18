using StrikeballApi.Application.DTOs.Auth;

namespace StrikeballApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors)> CreateUserAsync(string email, string password);
    Task<(bool Succeeded, string[] Errors)> AddToRoleAsync(Guid userId, string role);
    Task<bool> IsInRoleAsync(Guid userId, string role);
    Task<string[]> GetRolesAsync(Guid userId);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<Guid?> FindByEmailAsync(string email);
    Task UpdateRefreshTokenAsync(Guid userId, string? refreshToken, DateTime? expiryTime);
}
