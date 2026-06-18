using MediatR;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterGameHistoryQuery : IRequest<IEnumerable<GameHistoryDto>>
{
    public Guid AirsofterId { get; set; }
}
