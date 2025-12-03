using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Date)
            .IsRequired();

        builder.Property(g => g.Cost)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(g => g.CustomRules)
            .HasColumnType("text");

        builder.Property(g => g.UseFieldRules)
            .IsRequired();

        builder.HasOne(g => g.Field)
            .WithMany(f => f.Games)
            .HasForeignKey(g => g.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Organizer)
            .WithMany(o => o.OrganizedGames)
            .HasForeignKey(g => g.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.Date);
        builder.HasIndex(g => g.Status);
    }
}
