using MediatR;

namespace KomTracker.Application.Notifications.Component;

/// <summary>
/// A component's own data changed (created/edited seed, or a lifecycle transition) in a way that can affect its
/// stored mileage projection. Published after the mutation commits; consumed by
/// <see cref="ComponentMileageProjectionUpdater"/> (and available to future reactors, e.g. wear alerts).
/// </summary>
public class ComponentChangedNotification : INotification
{
    public int ComponentId { get; set; }
}
