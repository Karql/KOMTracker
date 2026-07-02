using KomTracker.Domain.Entities.Segment;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Segment;

public class KomTakeoverEntityTypeConfiguration
    : IEntityTypeConfiguration<KomTakeoverEntity>
{
    public void Configure(EntityTypeBuilder<KomTakeoverEntity> builder)
    {
        builder.ToTable("kom_takeover");

        builder.PrepareBaseColumns();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.TakenSegmentEffortId)
            .HasColumnName("taken_segment_effort_id");

        builder.Property(x => x.LostSegmentEffortId)
            .HasColumnName("lost_segment_effort_id");

        builder.Property(x => x.TakenKomsSummaryId)
            .HasColumnName("taken_koms_summary_id");

        builder.Property(x => x.LostKomsSummaryId)
            .HasColumnName("lost_koms_summary_id");

        builder.Property(x => x.TrackDate)
            .HasColumnName("track_date");

        builder.Property(x => x.Reverted)
            .HasColumnName("reverted")
            .IsRequired(true);

        // One takeover per winning effort (idempotency key for detection).
        builder.HasIndex(x => x.TakenSegmentEffortId).IsUnique();

        // Lookup when a ReturnedKom reverts a takeover.
        builder.HasIndex(x => x.LostSegmentEffortId);
    }
}
