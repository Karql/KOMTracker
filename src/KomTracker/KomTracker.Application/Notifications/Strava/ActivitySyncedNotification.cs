using MediatR;

namespace KomTracker.Application.Notifications.Strava;

/// <summary>
/// A single Strava activity was (re)synced. The gear it was recorded with tells us which bike — and thus which
/// components — need their mileage recomputed. Published after the upsert; consumed by
/// <see cref="Component.ComponentMileageProjectionUpdater"/>.
/// </summary>
public class ActivitySyncedNotification : INotification
{
    public int AthleteId { get; set; }

    public long ActivityId { get; set; }

    /// <summary>Strava gear id the activity was recorded with; null/empty when the ride had no gear assigned.</summary>
    public string? GearId { get; set; }
}
