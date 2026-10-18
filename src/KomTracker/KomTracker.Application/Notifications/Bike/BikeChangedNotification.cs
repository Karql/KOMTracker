using MediatR;

namespace KomTracker.Application.Notifications.Bike;

/// <summary>
/// A bike's mileage inputs changed — its own data (initial seed) was saved, or a Strava gear was linked / unlinked (so a
/// different set of rides now counts). Published after the mutation commits; consumed by
/// <see cref="Component.ComponentMileageProjectionUpdater"/>, which refreshes the mileage of the components installed on
/// it and of the services covering it or them.
/// </summary>
public class BikeChangedNotification : INotification
{
    public int BikeId { get; set; }
}
