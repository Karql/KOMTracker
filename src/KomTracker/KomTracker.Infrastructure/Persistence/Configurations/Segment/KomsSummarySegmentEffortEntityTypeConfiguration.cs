using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Segment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Persistence.Configurations.Segment;

public class KomsSummarySegmentEffortEntityTypeConfiguration
    : IEntityTypeConfiguration<KomsSummarySegmentEffortEntity>
{
    public void Configure(EntityTypeBuilder<KomsSummarySegmentEffortEntity> builder)
    {
        builder.ToTable("koms_summary_segment_effort");

        builder.Property(x => x.KomSummaryId)
            .HasColumnName("koms_summary_id");

        builder.Property(x => x.SegmentEffortId)
            .HasColumnName("segment_effort_id");

        builder.Property(x => x.Kom)
            .HasColumnName("kom")
            .IsRequired(true);

        builder.Property(x => x.NewKom)
            .HasColumnName("new_kom")
            .IsRequired(true);

        builder.Property(x => x.ImprovedKom)
            .HasColumnName("improved_kom")
            .IsRequired(true);

        builder.Property(x => x.LostKom)
            .HasColumnName("lost_kom")
            .IsRequired(true);
    }
}
