using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>What triggered an activity-sync-history row. Persisted by name (string); renaming needs a data migration.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivitySyncType
{
    Job,
    Webhook,
    Manual
}
