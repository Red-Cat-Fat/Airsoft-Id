using MediatR;

namespace StrikeballApi.Application.Features.Airsofters.Commands;

public class DeleteAirsofterCommand : IRequest
{
    public Guid Id { get; set; }
}
