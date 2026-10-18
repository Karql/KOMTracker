using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.ServiceRecord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.ServiceRecord;

public class ServiceTargetEntityTypeConfiguration
    : IEntityTypeConfiguration<ServiceTargetEntity>
{
    public void Configure(EntityTypeBuilder<ServiceTargetEntity> builder)
    {
        // A target is a bike XOR a component (D-SRV-1).
        builder.ToTable("service_target", "bt", t => t.HasCheckConstraint(
            "CK_service_target_bike_xor_component",
            "(bike_id IS NULL) <> (component_id IS NULL)"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id"); // DB-generated identity

        builder.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .IsRequired(true);

        builder.Property(x => x.BikeId)
            .HasColumnName("bike_id");

        builder.Property(x => x.ComponentId)
            .HasColumnName("component_id");

        builder.Property(x => x.DistanceKm)
            .HasColumnName("distance_km");

        builder.Property(x => x.MovingHours)
            .HasColumnName("moving_hours");

        builder.Property(x => x.ElevationM)
            .HasColumnName("elevation_m");

        builder.Property(x => x.ComputedAt)
            .HasColumnName("computed_at");

        // Referenceless FKs; Restrict — a bike/component a service covers can't be deleted (D-SRV-6).
        builder.HasOne<BikeEntity>().WithMany().HasForeignKey(x => x.BikeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ComponentEntity>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Restrict);

        // The same bike/component at most once per service.
        builder.HasIndex(x => new { x.ServiceId, x.BikeId }).IsUnique();
        builder.HasIndex(x => new { x.ServiceId, x.ComponentId }).IsUnique();
        builder.HasIndex(x => x.BikeId);
        builder.HasIndex(x => x.ComponentId);
    }
}
