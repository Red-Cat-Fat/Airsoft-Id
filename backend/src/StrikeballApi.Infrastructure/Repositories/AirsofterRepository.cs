using Microsoft.EntityFrameworkCore;
using StrikeballApi.Domain.Entities;
using StrikeballApi.Infrastructure.Data;

namespace StrikeballApi.Infrastructure.Repositories;

public class AirsofterRepository : Repository<Airsofter>, IAirsofterRepository
{
    public AirsofterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Airsofter?> GetByCallsignAsync(string callsign)
    {
        return await _dbSet
            .Include(a => a.Team)
            .FirstOrDefaultAsync(a => a.Callsign == callsign);
    }

    public async Task<Airsofter?> GetWithTeamAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Team)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Airsofter>> GetByTeamIdAsync(Guid teamId)
    {
        return await _dbSet
            .Where(a => a.TeamId == teamId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Airsofter>> GetWithViolationsAsync()
    {
        return await _dbSet
            .Include(a => a.Violations)
            .Include(a => a.Team)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Airsofter>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.Team)
            .ToListAsync();
    }

    public override async Task<Airsofter?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Team)
            .Include(a => a.Violations)
            .Include(a => a.GameParticipants)
                .ThenInclude(gp => gp.Game)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
