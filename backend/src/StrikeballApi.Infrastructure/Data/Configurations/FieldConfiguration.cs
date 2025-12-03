using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeballApi.Domain.Entities;

namespace StrikeballApi.Infrastructure.Data.Configurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.Latitude)
            .IsRequired()
            .HasPrecision(10, 8);

        builder.Property(f => f.Longitude)
            .IsRequired()
            .HasPrecision(11, 8);

        builder.Property(f => f.MapUrl)
            .HasMaxLength(500);

        builder.Property(f => f.Rules)
            .IsRequired()
            .HasColumnType("text");

        builder.HasIndex(f => new { f.Latitude, f.Longitude });
    }
}
