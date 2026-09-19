using MediatR;

namespace KomTracker.Application.Notifications.Strava;

/// <summary>
/// An athlete's activities were (re)synced — new rides may have arrived and stale ones may have been deleted, so the
/// mileage of components on that athlete's bikes may have changed. Published per athlete after each sync run; consumed
/// by <see cref="Component.ComponentMileageProjectionUpdater"/>.
/// </summary>
public class AthleteActivitiesSyncedNotification : INotification
{
    public int AthleteId { get; set; }
}
