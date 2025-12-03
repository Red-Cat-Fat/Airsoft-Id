using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Repositories;

public interface IAirsofterRepository : IRepository<Airsofter>
{
    Task<Airsofter?> GetByCallsignAsync(string callsign);
    Task<Airsofter?> GetWithTeamAsync(Guid id);
    Task<IEnumerable<Airsofter>> GetByTeamIdAsync(Guid teamId);
    Task<IEnumerable<Airsofter>> GetWithViolationsAsync();
}
