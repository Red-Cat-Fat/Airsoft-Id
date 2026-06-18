using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrikeballApi.Application.DTOs.Airsofters;
using StrikeballApi.Application.Features.Airsofters.Commands;
using StrikeballApi.Application.Features.Airsofters.Queries;

namespace StrikeballApi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AirsoftersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AirsoftersController> _logger;

    public AirsoftersController(IMediator mediator, ILogger<AirsoftersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AirsofterDto>>> GetAll([FromQuery] string? callsign = null)
    {
        var result = await _mediator.Send(new GetAirsoftersListQuery { Callsign = callsign });
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<AirsofterDetailsDto>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAirsofterByIdQuery { Id = id });
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}/game-history")]
    public async Task<ActionResult> GetGameHistory(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAirsofterGameHistoryQuery { AirsofterId = id });
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}/violations")]
    public async Task<ActionResult> GetViolations(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAirsofterViolationsQuery { AirsofterId = id });
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAirsofterCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Id mismatch" });
        }

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteAirsofterCommand { Id = id });
            return NoContent();
        }
        catch (Application.Common.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
    }
}
