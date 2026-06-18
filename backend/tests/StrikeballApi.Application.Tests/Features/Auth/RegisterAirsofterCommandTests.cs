using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.Features.Auth.Commands;
using StrikeballApi.Application.DTOs.Auth;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Auth;

public class RegisterAirsofterCommandTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly RegisterAirsofterCommandHandler _handler;
    private readonly TestDbContext _context;

    public RegisterAirsofterCommandTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"RegisterTest_{Guid.NewGuid()}")
            .Options;
        _context = new TestDbContext(options);

        _tokenServiceMock = new Mock<ITokenService>();
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new RegisterAirsofterCommandHandler(
            _context,
            _tokenServiceMock.Object,
            _identityServiceMock.Object,
            NullLogger<RegisterAirsofterCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAuthResponse()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, Array.Empty<string>()));
        _identityServiceMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);
        _identityServiceMock.Setup(x => x.AddToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        _tokenServiceMock.Setup(x => x.GenerateTokensAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AuthResponse
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Id = userId,
                    Email = "test@example.com",
                    Callsign = "TestUser",
                    Roles = new List<string> { "Airsofter" }
                }
            });

        var command = new RegisterAirsofterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Callsign = "TestUser"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.User.Callsign.Should().Be("TestUser");
    }
}
