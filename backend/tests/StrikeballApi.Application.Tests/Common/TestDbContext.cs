using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Application.Tests;

public class TestDbContext : DbContext, IApplicationDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Airsofter> Airsofters => Set<Airsofter>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameParticipant> GameParticipants => Set<GameParticipant>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Violation> Violations => Set<Violation>();
    public DbSet<FieldBannedTeam> FieldBannedTeams => Set<FieldBannedTeam>();
    public DbSet<OrganizerFieldAuthorization> OrganizerFieldAuthorizations => Set<OrganizerFieldAuthorization>();
}
