using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Domain.Entities;
using StrikeballApi.Infrastructure.Data;
using StrikeballApi.Infrastructure.Identity;
using StrikeballApi.WebApi.Models.Auth;
using StrikeballApi.WebApi.Services;

namespace StrikeballApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match" });
        }

        // Check if callsign is unique
        var existingAirsofter = await _context.Airsofters
            .FirstOrDefaultAsync(a => a.Callsign == request.Callsign);

        if (existingAirsofter != null)
        {
            return BadRequest(new { message = "Callsign is already taken" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Registration failed", errors = result.Errors });
        }

        // Create Airsofter profile
        var airsofter = new Airsofter
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Callsign = request.Callsign,
            UserId = user.Id
        };

        _context.Airsofters.Add(airsofter);
        await _context.SaveChangesAsync();

        // Assign default role
        await _userManager.AddToRoleAsync(user, "Airsofter");

        // Reload user with airsofter
        user = await _userManager.Users
            .Include(u => u.Airsofter)
            .FirstAsync(u => u.Id == user.Id);

        var authResponse = await _tokenService.GenerateTokensAsync(user);

        _logger.LogInformation("User {Email} registered successfully", user.Email);

        return Ok(authResponse);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.Airsofter)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var authResponse = await _tokenService.GenerateTokensAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return Ok(authResponse);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        return Ok(authResponse);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId != null && Guid.TryParse(userId, out var userGuid))
        {
            var user = await _userManager.FindByIdAsync(userGuid.ToString());

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged out successfully", user.Email);
            }
        }

        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var user = await _userManager.Users
            .Include(u => u.Airsofter)
            .FirstOrDefaultAsync(u => u.Id == userGuid);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserInfo
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Callsign = user.Airsofter?.Callsign ?? string.Empty,
            Roles = roles.ToList()
        });
    }
}
