using MediatR;

namespace KomTracker.Application.Notifications.ServiceRecord;

/// <summary>
/// A service was created or edited (date / targets may have changed). Published after the save commits; consumed by
/// <see cref="Component.ComponentMileageProjectionUpdater"/> to (re)compute its targets' mileage at the service date.
/// </summary>
public class ServiceRecordChangedNotification : INotification
{
    public int ServiceId { get; set; }
}
