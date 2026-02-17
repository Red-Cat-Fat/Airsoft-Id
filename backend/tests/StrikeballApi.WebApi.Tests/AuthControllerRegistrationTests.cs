using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StrikeballApi.Infrastructure.Data;
using StrikeballApi.Infrastructure.Identity;
using StrikeballApi.WebApi.Controllers;
using StrikeballApi.WebApi.Models.Auth;
using StrikeballApi.WebApi.Services;

namespace StrikeballApi.WebApi.Tests;

public class AuthControllerRegistrationTests
{
    [Fact]
    public async Task Register_AllowsMultipleUsersWithSameCallsign()
    {
        await using var scope = CreateScope();
        var controller = CreateController(scope.ServiceProvider);

        var firstResult = await controller.Register(new RegisterRequest
        {
            Email = "first@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1",
            FirstName = "First",
            LastName = "User",
            Callsign = "Ghost"
        });

        var secondResult = await controller.Register(new RegisterRequest
        {
            Email = "second@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1",
            FirstName = "Second",
            LastName = "User",
            Callsign = "Ghost"
        });

        Assert.IsType<OkObjectResult>(firstResult.Result);
        Assert.IsType<OkObjectResult>(secondResult.Result);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sameCallsignCount = await db.Airsofters.CountAsync(a => a.Callsign == "Ghost");
        Assert.Equal(2, sameCallsignCount);
    }

    [Fact]
    public async Task Register_CreatesRoleIfMissing_AndRegistersUser()
    {
        await using var scope = CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var controller = CreateController(scope.ServiceProvider);

        Assert.False(await roleManager.RoleExistsAsync("Airsofter"));

        var result = await controller.Register(new RegisterRequest
        {
            Email = "norole@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1",
            FirstName = "No",
            LastName = "Role",
            Callsign = "Falcon"
        });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(await roleManager.RoleExistsAsync("Airsofter"));

        var user = await userManager.FindByEmailAsync("norole@example.com");
        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user!, "Airsofter"));
    }

    private static AsyncServiceScope CreateScope()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"auth-tests-{Guid.NewGuid()}"));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        var provider = services.BuildServiceProvider();
        return provider.CreateAsyncScope();
    }

    private static AuthController CreateController(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        return new AuthController(
            userManager,
            signInManager,
            new FakeTokenService(),
            roleManager,
            db,
            NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private sealed class FakeTokenService : ITokenService
    {
        public ClaimsPrincipal? ValidateToken(string token) => null;

        public Task<AuthResponse> GenerateTokensAsync(ApplicationUser user)
        {
            return Task.FromResult(new AuthResponse
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Callsign = user.Airsofter?.Callsign ?? string.Empty,
                    Roles = new List<string> { "Airsofter" }
                }
            });
        }

        public Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            return Task.FromResult<AuthResponse?>(null);
        }
    }
}
