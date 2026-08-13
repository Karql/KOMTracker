using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Strava;

public class AthleteSyncEntityTypeConfiguration : IEntityTypeConfiguration<AthleteSyncEntity>
{
    public void Configure(EntityTypeBuilder<AthleteSyncEntity> builder)
    {
        builder.ToTable("athlete_sync", "strava");

        builder.PrepareBaseColumns();

        builder.HasOne<AthleteEntity>().WithMany().HasForeignKey(x => x.AthleteId);

        builder.HasKey(x => x.AthleteId);
        builder.Property(x => x.AthleteId).HasColumnName("athlete_id").ValueGeneratedNever();

        builder.Property(x => x.ActivitiesEnabled).HasColumnName("activities_enabled").IsRequired(true);
        builder.Property(x => x.BikesEnabled).HasColumnName("bikes_enabled").IsRequired(true);
    }
}
