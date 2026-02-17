using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StrikeballApi.Domain.Entities;
using StrikeballApi.Infrastructure.Repositories;
using StrikeballApi.WebApi.Models.Airsofters;

namespace StrikeballApi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AirsoftersController : ControllerBase
{
    private readonly IAirsofterRepository _repository;
    private readonly ILogger<AirsoftersController> _logger;

    public AirsoftersController(
        IAirsofterRepository repository,
        ILogger<AirsoftersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AirsofterDto>>> GetAll([FromQuery] string? callsign = null)
    {
        var airsofters = string.IsNullOrWhiteSpace(callsign)
            ? await _repository.GetAllAsync()
            : await _repository.SearchByCallsignAsync(callsign.Trim());

        var dtos = airsofters.Select(MapToDto);
        return Ok(dtos);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<AirsofterDto>> GetById(Guid id)
    {
        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(airsofter));
    }

    [AllowAnonymous]
    [HttpGet("callsign/{callsign}")]
    public async Task<ActionResult<AirsofterDto>> GetByCallsign(string callsign)
    {
        var airsofter = await _repository.GetByCallsignAsync(callsign);

        if (airsofter == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(airsofter));
    }

    [HttpPost]
    public async Task<ActionResult<AirsofterDto>> Create([FromBody] CreateAirsofterRequest request)
    {
        // Check if callsign already exists
        var existing = await _repository.GetByCallsignAsync(request.Callsign);
        if (existing != null)
        {
            return BadRequest(new { message = "Callsign already exists" });
        }

        var airsofter = new Airsofter
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Callsign = request.Callsign,
            KitDescription = request.KitDescription,
            AirsoftGunsList = request.AirsoftGunsList,
            TeamId = request.TeamId,
            UserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString())
        };

        var created = await _repository.AddAsync(airsofter);

        _logger.LogInformation("Airsofter {Callsign} created", created.Callsign);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAirsofterRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (User.Identity?.IsAuthenticated != true || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        var isAdministrator = User.IsInRole("Administrator");
        if (airsofter.UserId != userId && !isAdministrator)
        {
            return Forbid();
        }

        UpdateAirsofterFromRequest(airsofter, request);

        await _repository.UpdateAsync(airsofter);

        _logger.LogInformation("Airsofter {Id} updated", id);

        return NoContent();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/admin")]
    public async Task<IActionResult> UpdateAsAdministrator(Guid id, [FromBody] UpdateAirsofterRequest request)
    {
        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        UpdateAirsofterFromRequest(airsofter, request);

        await _repository.UpdateAsync(airsofter);

        _logger.LogInformation("Airsofter {Id} updated by administrator", id);

        return NoContent();
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(airsofter);

        _logger.LogInformation("Airsofter {Id} deleted", id);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id}/game-history")]
    public async Task<ActionResult> GetGameHistory(Guid id)
    {
        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        var games = airsofter.GameParticipants.Select(gp => new
        {
            gp.Game.Id,
            gp.Game.Date,
            gp.Game.Field.Name,
            gp.Status,
            gp.RegisteredAt
        });

        return Ok(games);
    }

    [AllowAnonymous]
    [HttpGet("{id}/violations")]
    public async Task<ActionResult> GetViolations(Guid id)
    {
        var airsofter = await _repository.GetByIdAsync(id);

        if (airsofter == null)
        {
            return NotFound();
        }

        var violations = airsofter.Violations.Select(v => new
        {
            v.Id,
            v.Description,
            v.Date,
            v.Severity,
            FieldName = v.Field.Name
        });

        return Ok(violations);
    }

    private static AirsofterDto MapToDto(Airsofter airsofter)
    {
        return new AirsofterDto
        {
            Id = airsofter.Id,
            FirstName = airsofter.FirstName,
            LastName = airsofter.LastName,
            MiddleName = airsofter.MiddleName,
            Callsign = airsofter.Callsign,
            KitDescription = airsofter.KitDescription,
            AirsoftGunsList = airsofter.AirsoftGunsList,
            TeamId = airsofter.TeamId,
            TeamName = airsofter.Team?.FullName,
            GamesPlayed = airsofter.GameParticipants?.Count ?? 0,
            ViolationsCount = airsofter.Violations?.Count ?? 0
        };
    }

    private static void UpdateAirsofterFromRequest(Airsofter airsofter, UpdateAirsofterRequest request)
    {
        airsofter.FirstName = request.FirstName;
        airsofter.LastName = request.LastName;
        airsofter.MiddleName = request.MiddleName;
        airsofter.KitDescription = request.KitDescription;
        airsofter.AirsoftGunsList = request.AirsoftGunsList;
        airsofter.TeamId = request.TeamId;
    }
}
