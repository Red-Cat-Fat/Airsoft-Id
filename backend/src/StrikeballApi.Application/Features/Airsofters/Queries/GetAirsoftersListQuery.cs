using MediatR;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsoftersListQuery : IRequest<IEnumerable<AirsofterDto>>
{
    public string? Callsign { get; set; }
}
