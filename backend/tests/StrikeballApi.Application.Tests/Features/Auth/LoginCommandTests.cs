using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.Features.Auth.Commands;
using StrikeballApi.Application.DTOs.Auth;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Auth;

public class LoginCommandTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new LoginCommandHandler(
            _tokenServiceMock.Object,
            _identityServiceMock.Object,
            NullLogger<LoginCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);
        _identityServiceMock.Setup(x => x.CheckPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _identityServiceMock.Setup(x => x.GetRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new[] { "Airsofter" });

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

        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorizedException()
    {
        _identityServiceMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Guid?)null);

        var command = new LoginCommand
        {
            Email = "invalid@example.com",
            Password = "WrongPassword"
        };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.UnauthorizedException>();
    }
}
