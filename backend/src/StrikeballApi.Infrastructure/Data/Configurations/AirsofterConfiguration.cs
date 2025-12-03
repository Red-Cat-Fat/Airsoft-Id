using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Data.Configurations;

public class AirsofterConfiguration : IEntityTypeConfiguration<Airsofter>
{
    public void Configure(EntityTypeBuilder<Airsofter> builder)
    {
        builder.ToTable("Airsofters");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.MiddleName)
            .HasMaxLength(100);

        builder.Property(a => a.Callsign)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.Callsign)
            .IsUnique();

        builder.Property(a => a.KitDescription)
            .HasColumnType("text");

        builder.Property(a => a.AirsoftGunsList)
            .HasColumnType("text");

        builder.HasOne(a => a.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Organizer)
            .WithOne(o => o.Airsofter)
            .HasForeignKey<Organizer>(o => o.AirsofterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
