using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Component;

public class InstallationEntityTypeConfiguration
    : IEntityTypeConfiguration<InstallationEntity>
{
    public void Configure(EntityTypeBuilder<InstallationEntity> builder)
    {
        builder.ToTable("installation", "bt");

        builder.PrepareBaseColumns();

        // Owner (scoping). Referenceless FK, cascade with the user.
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId);

        // Component + bike parents — referenceless; deletion is handled in app logic
        // (component delete blocked when history exists; bike delete removes its installations first).
        builder.HasOne<ComponentEntity>().WithMany().HasForeignKey(x => x.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<BikeEntity>().WithMany().HasForeignKey(x => x.BikeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id"); // DB-generated identity

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired(true);

        builder.Property(x => x.ComponentId)
            .HasColumnName("component_id")
            .IsRequired(true);

        builder.Property(x => x.BikeId)
            .HasColumnName("bike_id");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(true);

        builder.Property(x => x.DateFrom)
            .HasColumnName("date_from");

        builder.Property(x => x.DateTo)
            .HasColumnName("date_to");

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ManualDistanceKm)
            .HasColumnName("manual_distance_km");

        builder.Property(x => x.ManualMovingHours)
            .HasColumnName("manual_moving_hours");

        builder.Property(x => x.ManualElevationM)
            .HasColumnName("manual_elevation_m");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ComponentId);
        builder.HasIndex(x => x.BikeId);
    }
}
