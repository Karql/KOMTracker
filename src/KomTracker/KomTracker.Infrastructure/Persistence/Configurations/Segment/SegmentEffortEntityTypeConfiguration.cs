using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Segment;

public class SegmentEffortEntityTypeConfiguration
    : IEntityTypeConfiguration<SegmentEffortEntity>
{
    public void Configure(EntityTypeBuilder<SegmentEffortEntity> builder)
    {
        builder.ToTable("segment_effort");

        builder.PrepareBaseColumns();

        builder.HasOne<SegmentEntity>()
            .WithMany()
            .HasForeignKey(x => x.SegmentId);

        builder.HasOne<AthleteEntity>()
            .WithMany()
            .HasForeignKey(x => x.AthleteId);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // id from strava


        builder.Property(x => x.ActivityId)
            .HasColumnName("activity_id");

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id");

        builder.Property(x => x.SegmentId)
            .HasColumnName("segment_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.Property(x => x.ElapsedTime)
            .HasColumnName("elapsed_time");

        builder.Property(x => x.MovingTime)
            .HasColumnName("moving_time");

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date");

        builder.Property(x => x.StartDateLocal)
            .HasColumnName("start_date_local");

        builder.Property(x => x.Distance)
            .HasColumnName("distance");

        builder.Property(x => x.StartIndex)
            .HasColumnName("start_index");

        builder.Property(x => x.EndIndex)
            .HasColumnName("end_index");

        builder.Property(x => x.AverageCadence)
            .HasColumnName("average_cadence");

        builder.Property(x => x.DeviceWatts)
            .HasColumnName("device_watts");

        builder.Property(x => x.AverageWatts)
            .HasColumnName("average_watts");

        builder.Property(x => x.AverageHeartrate)
            .HasColumnName("average_heartrate");

        builder.Property(x => x.MaxHeartrate)
            .HasColumnName("max_heartrate");

        builder.Property(x => x.PrRank)
            .HasColumnName("pr_rank");

        builder.Property(x => x.KomRank)
            .HasColumnName("kom_rank");
    }
}
