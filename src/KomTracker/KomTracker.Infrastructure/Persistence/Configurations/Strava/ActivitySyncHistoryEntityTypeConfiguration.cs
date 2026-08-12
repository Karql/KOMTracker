using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Strava;

public class ActivitySyncHistoryEntityTypeConfiguration : IEntityTypeConfiguration<ActivitySyncHistoryEntity>
{
    public void Configure(EntityTypeBuilder<ActivitySyncHistoryEntity> builder)
    {
        builder.ToTable("activity_sync_history", "strava");

        builder.PrepareBaseColumns();

        builder.HasOne<AthleteEntity>().WithMany().HasForeignKey(x => x.AthleteId);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id"); // DB-generated

        builder.Property(x => x.AthleteId).HasColumnName("athlete_id");
        builder.Property(x => x.RunAt).HasColumnName("run_at");
        builder.Property(x => x.Duration).HasColumnName("duration");
        builder.Property(x => x.SyncFrom).HasColumnName("sync_from");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(100).IsRequired(true);
        builder.Property(x => x.UpsertedCount).HasColumnName("upserted_count");
        builder.Property(x => x.DeletedCount).HasColumnName("deleted_count");
        builder.Property(x => x.ActivitiesCount).HasColumnName("activities_count");

        builder.HasIndex(x => x.AthleteId);
    }
}
