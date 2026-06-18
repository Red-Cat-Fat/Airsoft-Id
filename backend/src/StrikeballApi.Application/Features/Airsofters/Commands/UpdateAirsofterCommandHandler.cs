using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrikeballApi.Application.Common.Interfaces;

namespace StrikeballApi.Application.Features.Airsofters.Commands;

public class UpdateAirsofterCommandHandler : IRequestHandler<UpdateAirsofterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateAirsofterCommandHandler> _logger;

    public UpdateAirsofterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateAirsofterCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(UpdateAirsofterCommand request, CancellationToken cancellationToken)
    {
        var airsofter = await _context.Airsofters
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (airsofter == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Airsofter), request.Id);
        }

        if (airsofter.UserId != _currentUser.UserId && !_currentUser.IsAdministrator)
        {
            throw new Common.Exceptions.UnauthorizedException("You do not have permission to update this airsofter");
        }

        airsofter.FirstName = request.FirstName;
        airsofter.LastName = request.LastName;
        airsofter.MiddleName = request.MiddleName;
        airsofter.KitDescription = request.KitDescription;
        airsofter.AirsoftGunsList = request.AirsoftGunsList;
        airsofter.TeamId = request.TeamId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Airsofter {Id} updated", request.Id);
    }
}
