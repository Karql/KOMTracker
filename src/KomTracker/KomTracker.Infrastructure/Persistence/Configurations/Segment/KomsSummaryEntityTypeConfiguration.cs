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

public class KomsSummaryEntityTypeConfiguration
    : IEntityTypeConfiguration<KomsSummaryEntity>
{
    public void Configure(EntityTypeBuilder<KomsSummaryEntity> builder)
    {
        builder.ToTable("koms_summary");

        builder.PrepareBaseColumns();

        builder.HasMany(x => x.SegmentEfforts)
            .WithMany(x => x.KomSummaries)
            .UsingEntity<KomsSummarySegmentEffortEntity>(
                x => x.HasOne(x => x.SegmentEffort).WithMany().HasForeignKey(x => x.SegmentEffortId),
                x => x.HasOne(x => x.KomsSummary).WithMany().HasForeignKey(x => x.KomSummaryId)
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

        builder.Property(x => x.ImprovedKoms)
            .HasColumnName("improved_koms");

        builder.Property(x => x.LostKoms)
            .HasColumnName("lost_koms");

        builder.Property(x => x.ReturnedKoms)
            .HasColumnName("returned_koms");
    }
}
