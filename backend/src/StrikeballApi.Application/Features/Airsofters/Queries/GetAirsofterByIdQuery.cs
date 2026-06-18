using MediatR;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterByIdQuery : IRequest<AirsofterDetailsDto>
{
    public Guid Id { get; set; }
}
