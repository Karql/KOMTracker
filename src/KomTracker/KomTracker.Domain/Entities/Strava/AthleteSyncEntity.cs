#nullable enable
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>
/// Per-athlete Strava sync capability gate. Table: strava.athlete_sync (one row per athlete).
/// Generic on purpose (an athlete may sync several things) — one boolean per capability.
/// The activity-sync job processes only athletes with <see cref="ActivitiesEnabled"/> = true.
/// Per-run telemetry lives in strava.activity_sync_history.
/// </summary>
public class AthleteSyncEntity : BaseEntity
{
    public int AthleteId { get; set; }        // key + FK -> athlete
    public bool ActivitiesEnabled { get; set; }

    /// <summary>Set true once the athlete has run a Strava gear (bikes) sync. Distinguishes
    /// "never synced" from "synced but has no bikes on Strava" for the UI empty-state.</summary>
    public bool BikesEnabled { get; set; }
}
