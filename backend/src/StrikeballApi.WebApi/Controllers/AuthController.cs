using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Auth;
using StrikeballApi.Application.Features.Auth.Commands;
using StrikeballApi.Application.Features.Auth.Queries;

namespace StrikeballApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ICurrentUserService currentUser, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterAirsofterCommand command)
    {
        if (command.Password != command.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match" });
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = _currentUser.UserId;
        if (userId.HasValue)
        {
            await _mediator.Send(new LogoutCommand { UserId = userId.Value });
            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }

        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId.Value });
            return Ok(result);
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Unauthorized();
        }
    }
}
