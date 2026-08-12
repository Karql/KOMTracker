#nullable enable
using System;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>
/// One row per activity-sync run per athlete (full history — for "last N syncs" on the UI).
/// Table: strava.activity_sync_history.
/// </summary>
public class ActivitySyncHistoryEntity : BaseEntity
{
    public int Id { get; set; }               // DB-generated
    public int AthleteId { get; set; }        // FK -> athlete

    public DateTime RunAt { get; set; }       // when the sync run happened, any outcome (UTC)
    public TimeSpan Duration { get; set; }    // how long the run took (diagnostics)

    /// <summary>Window start (UTC). Null ⇒ full pull; a date ⇒ synced from that date (shown on the UI).</summary>
    public DateTime? SyncFrom { get; set; }

    public string Status { get; set; } = default!;   // Ok / Error / NoValidToken / RateLimited
    public int UpsertedCount { get; set; }
    public int DeletedCount { get; set; }

    /// <summary>Total activities stored for the athlete AFTER this run (running snapshot; diagnostics).
    /// Null when the run didn't complete a fetch (NoValidToken / Error / RateLimited).</summary>
    public int? ActivitiesCount { get; set; }
}
