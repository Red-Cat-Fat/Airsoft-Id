using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StrikeballApi.Application.Features.Auth.Commands;
using Xunit;

namespace StrikeballApi.WebApi.Tests.IntegrationTests.Auth;

public class LoginTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginCommand
        {
            Email = "nonexistent@test.com",
            Password = "WrongPassword123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
