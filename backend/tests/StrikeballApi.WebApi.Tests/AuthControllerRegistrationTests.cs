using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.Features.Auth.Commands;
using StrikeballApi.WebApi.Controllers;
using Xunit;

namespace StrikeballApi.WebApi.Tests;

public class AuthControllerRegistrationTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly AuthController _controller;

    public AuthControllerRegistrationTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _controller = new AuthController(
            _mediatorMock.Object,
            _currentUserMock.Object,
            NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Register_PasswordsDoNotMatch_ReturnsBadRequest()
    {
        var command = new RegisterAirsofterCommand
        {
            Email = "test@example.com",
            Password = "Password1",
            ConfirmPassword = "DifferentPassword",
            FirstName = "Test",
            LastName = "User",
            Callsign = "Tester"
        };

        var result = await _controller.Register(command);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
}
