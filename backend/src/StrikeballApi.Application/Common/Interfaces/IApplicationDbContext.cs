using Microsoft.EntityFrameworkCore;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Airsofter> Airsofters { get; }
    DbSet<Team> Teams { get; }
    DbSet<Field> Fields { get; }
    DbSet<Game> Games { get; }
    DbSet<GameParticipant> GameParticipants { get; }
    DbSet<Organizer> Organizers { get; }
    DbSet<Violation> Violations { get; }
    DbSet<FieldBannedTeam> FieldBannedTeams { get; }
    DbSet<OrganizerFieldAuthorization> OrganizerFieldAuthorizations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
