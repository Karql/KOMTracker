#nullable enable
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>
/// A raw Strava webhook event as received (D-12 inbox). We only collect + store here; a later worker drains
/// unprocessed rows. Table: strava.webhook_event. Fields mirror the Strava push payload
/// (https://developers.strava.com/docs/webhooks/). No FK to athlete — owner_id/object_id are raw Strava ids that
/// need not map to a tracked athlete.
/// </summary>
public class WebhookEventEntity : BaseEntity
{
    public int Id { get; set; }                       // DB-generated

    /// <summary>"activity" or "athlete".</summary>
    public string ObjectType { get; set; } = default!;

    /// <summary>Activity id (activity events) or athlete id (athlete events).</summary>
    public long ObjectId { get; set; }

    /// <summary>"create", "update", or "delete".</summary>
    public string AspectType { get; set; } = default!;

    /// <summary>The raw "updates" hash as JSON (update events / deauthorization); null otherwise.</summary>
    public string? Updates { get; set; }

    /// <summary>The athlete's id.</summary>
    public long OwnerId { get; set; }

    /// <summary>The push subscription id that delivered this event.</summary>
    public int SubscriptionId { get; set; }

    /// <summary>Event time as sent by Strava (Unix epoch seconds). AuditCD is our received-at time.</summary>
    public long EventTime { get; set; }

    /// <summary>False until a future worker handles it (D-12). Not processed yet in this step.</summary>
    public bool Processed { get; set; }
}
