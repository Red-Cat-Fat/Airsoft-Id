using MediatR;
using Microsoft.Extensions.Logging;
using StrikeballApi.Application.Common.Interfaces;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IApplicationDbContext context, ILogger<LogoutCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Airsofters
            .FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user != null)
        {
            _logger.LogInformation("User {UserId} logged out", request.UserId);
        }
    }
}
