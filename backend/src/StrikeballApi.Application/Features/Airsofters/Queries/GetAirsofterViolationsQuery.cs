using MediatR;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterViolationsQuery : IRequest<IEnumerable<ViolationDto>>
{
    public Guid AirsofterId { get; set; }
}
