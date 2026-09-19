using MediatR;

namespace KomTracker.Application.Notifications.Component;

/// <summary>
/// An installation of a component changed (installed / moved / removed / edited / deleted). The installed component's
/// effective windows — and thus its (and any nested child's) mileage — may have shifted. Published after the mutation
/// commits; consumed by <see cref="ComponentMileageProjectionUpdater"/>.
/// </summary>
public class InstallationChangedNotification : INotification
{
    /// <summary>The component whose installation changed. The updater expands to its children (current + historical).</summary>
    public int ComponentId { get; set; }
}
