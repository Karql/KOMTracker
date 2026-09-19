using KomTracker.Domain.Entities.Component;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Component;

public class ComponentMileageEntityTypeConfiguration
    : IEntityTypeConfiguration<ComponentMileageEntity>
{
    public void Configure(EntityTypeBuilder<ComponentMileageEntity> builder)
    {
        builder.ToTable("component_mileage", "bt");

        builder.PrepareBaseColumns();

        // 1:1 with the component — PK == FK, cascade-deleted with the component (referenceless).
        builder.HasOne<ComponentEntity>().WithMany().HasForeignKey(x => x.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(x => x.ComponentId);

        builder.Property(x => x.ComponentId)
            .HasColumnName("component_id")
            .ValueGeneratedNever();

        builder.Property(x => x.TotalDistanceKm)
            .HasColumnName("total_distance_km");

        builder.Property(x => x.TotalMovingHours)
            .HasColumnName("total_moving_hours");

        builder.Property(x => x.TotalElevationM)
            .HasColumnName("total_elevation_m");

        builder.Property(x => x.AttributedActivityCount)
            .HasColumnName("attributed_activity_count");

        builder.Property(x => x.ComputedAt)
            .HasColumnName("computed_at");
    }
}
