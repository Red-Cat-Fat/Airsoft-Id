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
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
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

        await using var transaction = await _context.Database.BeginTransactionAsync();
        ApplicationUser? user = null;

        try
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
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
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Airsofter");
            if (!addRoleResult.Succeeded)
            {
                var roleCreated = await EnsureRoleExistsAsync("Airsofter");
                if (!roleCreated)
                {
                    throw new InvalidOperationException("Не удалось назначить роль новому пользователю. Проверьте настройки ролей на сервере.");
                }

                addRoleResult = await _userManager.AddToRoleAsync(user, "Airsofter");
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError("Unable to assign Airsofter role to user {Email}: {Errors}", request.Email,
                        string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException("Не удалось назначить роль новому пользователю. Проверьте настройки ролей на сервере.");
                }
            }

            // Reload user with airsofter
            user = await _userManager.Users
                .Include(u => u.Airsofter)
                .FirstAsync(u => u.Id == user.Id);

            var authResponse = await _tokenService.GenerateTokensAsync(user);

            await transaction.CommitAsync();

            _logger.LogInformation("User {Email} registered successfully", user.Email);

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (user != null)
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (existingUser != null)
                {
                    var deleteResult = await _userManager.DeleteAsync(existingUser);
                    if (!deleteResult.Succeeded)
                    {
                        _logger.LogError("Unable to cleanup user {Email} after failed registration: {Errors}", request.Email,
                            string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            _logger.LogError(ex, "User registration failed for {Email}", request.Email);

            if (ex is InvalidOperationException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Registration failed due to server error" });
        }
    }

    private async Task<bool> EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return true;
        }

        var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole(roleName));
        if (!createRoleResult.Succeeded)
        {
            _logger.LogError("Unable to create role {Role}: {Errors}", roleName,
                string.Join(", ", createRoleResult.Errors.Select(e => e.Description)));
            return false;
        }

        return true;
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
