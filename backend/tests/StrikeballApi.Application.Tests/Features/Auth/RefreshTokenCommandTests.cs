using Moq;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.Features.Auth.Commands;
using StrikeballApi.Application.DTOs.Auth;
using Xunit;
using FluentAssertions;

namespace StrikeballApi.Application.Tests.Features.Auth;

public class RefreshTokenCommandTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new RefreshTokenCommandHandler(_tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsAuthResponse()
    {
        _tokenServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AuthResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    Callsign = "TestUser",
                    Roles = new List<string> { "Airsofter" }
                }
            });

        var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsUnauthorizedException()
    {
        _tokenServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((AuthResponse)null!);

        var command = new RefreshTokenCommand { RefreshToken = "invalid-token" };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Application.Common.Exceptions.UnauthorizedException>();
    }
}
