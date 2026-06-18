using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StrikeballApi.Application.DTOs.Auth;
using StrikeballApi.Application.Features.Auth.Commands;
using Xunit;

namespace StrikeballApi.WebApi.Tests.IntegrationTests.Auth;

public class RegisterTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithTokens()
    {
        var request = new RegisterAirsofterCommand
        {
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Callsign = "TestUser"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.User.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        var request = new RegisterAirsofterCommand
        {
            Email = "test@test.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword",
            FirstName = "Test",
            LastName = "User",
            Callsign = "TestUser"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
