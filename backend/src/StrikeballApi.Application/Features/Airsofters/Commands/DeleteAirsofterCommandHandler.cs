using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrikeballApi.Application.Common.Interfaces;

namespace StrikeballApi.Application.Features.Airsofters.Commands;

public class DeleteAirsofterCommandHandler : IRequestHandler<DeleteAirsofterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteAirsofterCommandHandler> _logger;

    public DeleteAirsofterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<DeleteAirsofterCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteAirsofterCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdministrator)
        {
            throw new Common.Exceptions.UnauthorizedException("Only administrators can delete airsofters");
        }

        var airsofter = await _context.Airsofters
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (airsofter == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Airsofter), request.Id);
        }

        _context.Airsofters.Remove(airsofter);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Airsofter {Id} deleted", request.Id);
    }
}
