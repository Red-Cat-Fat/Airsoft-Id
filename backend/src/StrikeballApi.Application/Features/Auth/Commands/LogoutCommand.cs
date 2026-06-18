using MediatR;

namespace StrikeballApi.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest
{
    public Guid UserId { get; set; }
}
