using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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
        await using var testScope = await CreateScopeAsync();
        var controller = CreateController(testScope.ServiceProvider);

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

        var db = testScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sameCallsignCount = await db.Airsofters.CountAsync(a => a.Callsign == "Ghost");
        Assert.Equal(2, sameCallsignCount);
    }

    [Fact]
    public async Task Register_CreatesRoleIfMissing_AndRegistersUser()
    {
        await using var testScope = await CreateScopeAsync();
        var roleManager = testScope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = testScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var controller = CreateController(testScope.ServiceProvider);

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

    [Fact]
    public async Task Register_RollsBackUserAndProfile_WhenRoleAssignmentFails()
    {
        await using var testScope = await CreateScopeAsync();
        var db = testScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var roleStore = new RoleStore<ApplicationRole, ApplicationDbContext, Guid>(db);
        var mockedRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<ApplicationRole>>.Instance);

        mockedRoleManager.Setup(r => r.RoleExistsAsync("Airsofter"))
            .ReturnsAsync(false);
        mockedRoleManager.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role creation failed" }));

        var controller = CreateController(testScope.ServiceProvider, mockedRoleManager.Object);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "fail-role@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1",
            FirstName = "Role",
            LastName = "Failure",
            Callsign = "Broken"
        });

        var errorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

        var userCount = await db.Users.CountAsync(u => u.Email == "fail-role@example.com");
        var airsofterCount = await db.Airsofters.CountAsync(a => a.Callsign == "Broken");

        Assert.Equal(0, userCount);
        Assert.Equal(0, airsofterCount);
    }

    private static async Task<TestScope> CreateScopeAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connection));

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
        var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        return new TestScope(scope, connection);
    }

    private static AuthController CreateController(IServiceProvider serviceProvider, RoleManager<ApplicationRole>? roleManagerOverride = null)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var roleManager = roleManagerOverride ?? serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
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

    private sealed class TestScope : IAsyncDisposable
    {
        private readonly AsyncServiceScope _scope;
        private readonly SqliteConnection _connection;

        public TestScope(AsyncServiceScope scope, SqliteConnection connection)
        {
            _scope = scope;
            _connection = connection;
        }

        public IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public async ValueTask DisposeAsync()
        {
            await _scope.DisposeAsync();
            await _connection.DisposeAsync();
        }
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
