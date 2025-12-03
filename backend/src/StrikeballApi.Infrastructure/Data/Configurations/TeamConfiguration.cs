using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Abbreviation)
            .HasMaxLength(20);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);
    }
}
