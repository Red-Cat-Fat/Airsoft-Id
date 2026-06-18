using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StrikeballApi.Application.Common.Interfaces;
using StrikeballApi.Domain.Entities;
using StrikeballApi.Infrastructure.Identity;

namespace StrikeballApi.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Airsofter> Airsofters => Set<Airsofter>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Violation> Violations => Set<Violation>();
    public DbSet<GameParticipant> GameParticipants => Set<GameParticipant>();
    public DbSet<OrganizerFieldAuthorization> OrganizerFieldAuthorizations => Set<OrganizerFieldAuthorization>();
    public DbSet<FieldBannedTeam> FieldBannedTeams => Set<FieldBannedTeam>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
