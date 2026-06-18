using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterByIdQueryHandler : IRequestHandler<GetAirsofterByIdQuery, AirsofterDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAirsofterByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AirsofterDetailsDto> Handle(GetAirsofterByIdQuery request, CancellationToken cancellationToken)
    {
        var airsofter = await _context.Airsofters
            .Include(a => a.Team)
            .Include(a => a.GameParticipants)
                .ThenInclude(gp => gp.Game)
                    .ThenInclude(g => g.Field)
            .Include(a => a.Violations)
                .ThenInclude(v => v.Field)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (airsofter == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Airsofter), request.Id);
        }

        return _mapper.Map<AirsofterDetailsDto>(airsofter);
    }
}
