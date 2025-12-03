using Microsoft.AspNetCore.Identity;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation property
    public Airsofter? Airsofter { get; set; }
}
