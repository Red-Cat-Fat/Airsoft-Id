using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Application.DTOs.Airsofters;

namespace StrikeballApi.Application.Features.Airsofters.Queries;

public class GetAirsofterViolationsQueryHandler : IRequestHandler<GetAirsofterViolationsQuery, IEnumerable<ViolationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAirsofterViolationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ViolationDto>> Handle(GetAirsofterViolationsQuery request, CancellationToken cancellationToken)
    {
        var airsofter = await _context.Airsofters
            .Include(a => a.Violations)
                .ThenInclude(v => v.Field)
            .FirstOrDefaultAsync(a => a.Id == request.AirsofterId, cancellationToken);

        if (airsofter == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Airsofter), request.AirsofterId);
        }

        return _mapper.Map<IEnumerable<ViolationDto>>(airsofter.Violations);
    }
}
