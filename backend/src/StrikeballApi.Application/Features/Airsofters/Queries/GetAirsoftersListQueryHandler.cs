using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsoftersListQueryHandler : IRequestHandler<GetAirsoftersListQuery, IEnumerable<AirsofterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAirsoftersListQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AirsofterDto>> Handle(GetAirsoftersListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Airsofters
            .Include(a => a.Team)
            .Include(a => a.GameParticipants)
            .Include(a => a.Violations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Callsign))
        {
            query = query.Where(a => a.Callsign.Contains(request.Callsign.Trim()));
        }

        var airsofters = await query
            .OrderBy(a => a.Callsign)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<AirsofterDto>>(airsofters);
    }
}
