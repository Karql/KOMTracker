using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.DAL.EntityConfigurations.Segment;

public class KomsSummarySegmentEffortModelTypeConfiguration
    : IEntityTypeConfiguration<KomsSummarySegmentEffortModel>
{
    public void Configure(EntityTypeBuilder<KomsSummarySegmentEffortModel> builder)
    {
        builder.ToTable("koms_summary_segment_effort");

        builder.Property(x => x.KomSummaryId)
            .HasColumnName("koms_summary_id");

        builder.Property(x => x.SegmentEffortId)
            .HasColumnName("segment_effort_id");
    }
}
