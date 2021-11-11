using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Segment;

public class SegmentModelTypeConfiguration
    : IEntityTypeConfiguration<SegmentModel>
{
    public void Configure(EntityTypeBuilder<SegmentModel> builder)
    {
        builder.ToTable("segment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // id from strava

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.Property(x => x.ActivityType)
            .HasColumnName("activity_type")
            .HasMaxLength(20);

        builder.Property(x => x.Distance)
            .HasColumnName("distance");

        builder.Property(x => x.AverageGrade)
            .HasColumnName("average_grade");

        builder.Property(x => x.MaximumGrade)
            .HasColumnName("maximum_grade");

        builder.Property(x => x.ElevationHigh)
            .HasColumnName("elevation_high");

        builder.Property(x => x.ElevationLow)
            .HasColumnName("elevation_low");

        builder.Property(x => x.StartLatitude)
            .HasColumnName("start_latitude");

        builder.Property(x => x.StartLongitude)
            .HasColumnName("start_longitude");

        builder.Property(x => x.EndLatitude)
            .HasColumnName("end_latitude");

        builder.Property(x => x.EndLongitude)
            .HasColumnName("end_longitude");

        builder.Property(x => x.ClimbCategory)
            .HasColumnName("climb_category");

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(255);

        builder.Property(x => x.Country)
            .HasColumnName("country")
            .HasMaxLength(255);

        builder.Property(x => x.Private)
            .HasColumnName("private");

        builder.Property(x => x.Hazardous)
            .HasColumnName("hazardous");

        builder.Property(x => x.Starred)
            .HasColumnName("starred");
    }
}
