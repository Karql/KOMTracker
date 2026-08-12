using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Strava;

public class ActivityEntityTypeConfiguration : IEntityTypeConfiguration<ActivityEntity>
{
    public void Configure(EntityTypeBuilder<ActivityEntity> builder)
    {
        builder.ToTable("activity", "strava");

        builder.PrepareBaseColumns();

        builder.HasOne<AthleteEntity>().WithMany().HasForeignKey(x => x.AthleteId);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); // id from strava

        builder.Property(x => x.AthleteId).HasColumnName("athlete_id");
        builder.Property(x => x.GearId).HasColumnName("gear_id").HasMaxLength(50);

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired(true);
        builder.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(255);
        builder.Property(x => x.UploadId).HasColumnName("upload_id");

        builder.Property(x => x.Distance).HasColumnName("distance");
        builder.Property(x => x.MovingTime).HasColumnName("moving_time");
        builder.Property(x => x.ElapsedTime).HasColumnName("elapsed_time");
        builder.Property(x => x.TotalElevationGain).HasColumnName("total_elevation_gain");
        builder.Property(x => x.ElevHigh).HasColumnName("elev_high");
        builder.Property(x => x.ElevLow).HasColumnName("elev_low");

        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50);
        builder.Property(x => x.SportType).HasColumnName("sport_type").HasMaxLength(50).IsRequired(true);
        builder.Property(x => x.WorkoutType).HasColumnName("workout_type");

        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.Timezone).HasColumnName("timezone").HasMaxLength(100);
        builder.Property(x => x.UtcOffset).HasColumnName("utc_offset");

        builder.Property(x => x.Trainer).HasColumnName("trainer");
        builder.Property(x => x.Commute).HasColumnName("commute");
        builder.Property(x => x.Manual).HasColumnName("manual");
        builder.Property(x => x.Private).HasColumnName("private");
        builder.Property(x => x.Flagged).HasColumnName("flagged");
        builder.Property(x => x.Visibility).HasColumnName("visibility").HasMaxLength(50);

        builder.Property(x => x.AverageSpeed).HasColumnName("average_speed");
        builder.Property(x => x.MaxSpeed).HasColumnName("max_speed");
        builder.Property(x => x.AverageCadence).HasColumnName("average_cadence");
        builder.Property(x => x.AverageTemp).HasColumnName("average_temp");
        builder.Property(x => x.AverageWatts).HasColumnName("average_watts");
        builder.Property(x => x.WeightedAverageWatts).HasColumnName("weighted_average_watts");
        builder.Property(x => x.MaxWatts).HasColumnName("max_watts");
        builder.Property(x => x.DeviceWatts).HasColumnName("device_watts");
        builder.Property(x => x.Kilojoules).HasColumnName("kilojoules");
        builder.Property(x => x.HasHeartrate).HasColumnName("has_heartrate");
        builder.Property(x => x.AverageHeartrate).HasColumnName("average_heartrate");
        builder.Property(x => x.MaxHeartrate).HasColumnName("max_heartrate");
        builder.Property(x => x.SufferScore).HasColumnName("suffer_score");

        builder.Property(x => x.AchievementCount).HasColumnName("achievement_count");
        builder.Property(x => x.KudosCount).HasColumnName("kudos_count");
        builder.Property(x => x.CommentCount).HasColumnName("comment_count");
        builder.Property(x => x.AthleteCount).HasColumnName("athlete_count");
        builder.Property(x => x.PhotoCount).HasColumnName("photo_count");
        builder.Property(x => x.TotalPhotoCount).HasColumnName("total_photo_count");
        builder.Property(x => x.PrCount).HasColumnName("pr_count");

        builder.Property(x => x.SummaryPolyline).HasColumnName("summary_polyline");
        builder.Property(x => x.StartLat).HasColumnName("start_lat");
        builder.Property(x => x.StartLng).HasColumnName("start_lng");
        builder.Property(x => x.EndLat).HasColumnName("end_lat");
        builder.Property(x => x.EndLng).HasColumnName("end_lng");
        builder.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(255);

        builder.HasIndex(x => x.AthleteId);
        builder.HasIndex(x => x.GearId);
    }
}
