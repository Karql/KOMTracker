using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KomTracker.Infrastructure.Persistence.Configurations.Strava;

public class WebhookEventEntityTypeConfiguration : IEntityTypeConfiguration<WebhookEventEntity>
{
    public void Configure(EntityTypeBuilder<WebhookEventEntity> builder)
    {
        builder.ToTable("webhook_event", "strava");

        builder.PrepareBaseColumns();

        // No FK to athlete on purpose — owner_id/object_id are raw Strava ids that may not map to a tracked athlete.

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id"); // DB-generated

        builder.Property(x => x.ObjectType).HasColumnName("object_type").HasMaxLength(50).IsRequired(true);
        builder.Property(x => x.ObjectId).HasColumnName("object_id");
        builder.Property(x => x.AspectType).HasColumnName("aspect_type").HasMaxLength(50).IsRequired(true);
        builder.Property(x => x.Updates).HasColumnName("updates").HasColumnType("jsonb");
        builder.Property(x => x.OwnerId).HasColumnName("owner_id");
        builder.Property(x => x.SubscriptionId).HasColumnName("subscription_id");
        builder.Property(x => x.EventTime).HasColumnName("event_time");
        builder.Property(x => x.Processed).HasColumnName("processed").HasDefaultValue(false);

        // The future worker queries unprocessed rows; event_time helps ad-hoc analysis.
        builder.HasIndex(x => x.Processed);
        builder.HasIndex(x => x.EventTime);
    }
}
