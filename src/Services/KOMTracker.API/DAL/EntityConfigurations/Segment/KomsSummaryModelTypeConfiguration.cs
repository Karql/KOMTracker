using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Segment;

public class KomsSummaryModelTypeConfiguration
    : IEntityTypeConfiguration<KomsSummaryModel>
{
    public void Configure(EntityTypeBuilder<KomsSummaryModel> builder)
    {
        builder.ToTable("koms_summary");

        builder.HasMany(x => x.SegmentEfforts)
            .WithMany(x => x.KomSummaries)
            .UsingEntity<KomsSummarySegmentEffortModel>(
                x => x.HasOne<SegmentEffortModel>().WithMany().HasForeignKey(x => x.SegmentEffortId),
                x => x.HasOne<KomsSummaryModel>().WithMany().HasForeignKey(x => x.KomSummaryId)
            );

        builder.HasIndex(x => new { x.AthleteId, x.TrackDate });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.AthleteId)
            .HasColumnName("athlete_id");

        builder.Property(x => x.TrackDate)
            .HasColumnName("track_date");

        builder.Property(x => x.Koms)
            .HasColumnName("koms");

        builder.Property(x => x.NewKoms)
            .HasColumnName("new_koms");

        builder.Property(x => x.LostKoms)
            .HasColumnName("lost_koms");
    }
}
