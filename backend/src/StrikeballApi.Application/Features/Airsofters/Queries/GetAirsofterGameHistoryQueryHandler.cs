using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterGameHistoryQueryHandler : IRequestHandler<GetAirsofterGameHistoryQuery, IEnumerable<GameHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAirsofterGameHistoryQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GameHistoryDto>> Handle(GetAirsofterGameHistoryQuery request, CancellationToken cancellationToken)
    {
        var airsofter = await _context.Airsofters
            .Include(a => a.GameParticipants)
                .ThenInclude(gp => gp.Game)
                    .ThenInclude(g => g.Field)
            .FirstOrDefaultAsync(a => a.Id == request.AirsofterId, cancellationToken);

        if (airsofter == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Airsofter), request.AirsofterId);
        }

        return _mapper.Map<IEnumerable<GameHistoryDto>>(airsofter.GameParticipants);
    }
}
