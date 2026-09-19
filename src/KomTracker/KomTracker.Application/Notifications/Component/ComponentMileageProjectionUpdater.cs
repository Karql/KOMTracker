using KomTracker.Application.Commands.Component;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Notifications.Strava;
using KomTracker.Domain.Entities.Bike;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KomTracker.Application.Notifications.Component;

/// <summary>
/// The single reactor that keeps the stored component-mileage projection (<c>bt.component_mileage</c>) fresh. It
/// subscribes to the domain facts that can change a component's attribution — <see cref="ComponentChangedNotification"/>,
/// <see cref="InstallationChangedNotification"/>, <see cref="AthleteActivitiesSyncedNotification"/>,
/// <see cref="ActivitySyncedNotification"/> — resolves each to the affected component ids, and recomputes-from-source.
/// <para>
/// The mutation/sync commands only announce what happened; the knowledge of <em>which</em> components that touches
/// (athlete → bikes → components, gear → bike → components) lives here, next to the mileage code, then dispatches the
/// canonical <see cref="RecalculateComponentsMileageCommand"/> (which owns child expansion + recompute + upsert).
/// </para>
/// <para>
/// Best-effort: a recompute failure is logged and swallowed so it never breaks the originating mutation or a sibling
/// handler — the projection is always rebuildable via <c>PUT admin/recalculate-component-mileage</c>.
/// </para>
/// </summary>
public class ComponentMileageProjectionUpdater :
    INotificationHandler<ComponentChangedNotification>,
    INotificationHandler<InstallationChangedNotification>,
    INotificationHandler<AthleteActivitiesSyncedNotification>,
    INotificationHandler<ActivitySyncedNotification>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ILogger<ComponentMileageProjectionUpdater> _logger;

    public ComponentMileageProjectionUpdater(IKOMUnitOfWork komUoW, IUserService userService, IMediator mediator, ILogger<ComponentMileageProjectionUpdater> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(ComponentChangedNotification notification, CancellationToken cancellationToken)
        => RecalculateSafelyAsync(new[] { notification.ComponentId }, $"component {notification.ComponentId}", cancellationToken);

    public Task Handle(InstallationChangedNotification notification, CancellationToken cancellationToken)
        => RecalculateSafelyAsync(new[] { notification.ComponentId }, $"installation change on component {notification.ComponentId}", cancellationToken);

    public async Task Handle(AthleteActivitiesSyncedNotification notification, CancellationToken cancellationToken)
    {
        var componentIds = await ResolveAthleteComponentIdsAsync(notification.AthleteId);
        await RecalculateSafelyAsync(componentIds, $"athlete {notification.AthleteId} activities sync", cancellationToken);
    }

    public async Task Handle(ActivitySyncedNotification notification, CancellationToken cancellationToken)
    {
        var componentIds = await ResolveGearComponentIdsAsync(notification.GearId);
        await RecalculateSafelyAsync(componentIds, $"activity {notification.ActivityId} sync (gear {notification.GearId})", cancellationToken);
    }

    // Athlete → owning user → their bikes → components installed (ever) on those bikes.
    private async Task<IReadOnlyCollection<int>> ResolveAthleteComponentIdsAsync(int athleteId)
    {
        var userId = (await _userService.GetUserAsync(athleteId))?.Id;
        if (userId is null)
        {
            return Array.Empty<int>();
        }

        var bikeIds = (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(userId, includeInactive: true))
            .Select(b => b.Id)
            .ToList();
        if (bikeIds.Count == 0)
        {
            return Array.Empty<int>();
        }

        return (await _komUoW.GetRepository<IInstallationRepository>().GetComponentIdsByBikesAsync(bikeIds)).ToList();
    }

    // Gear → linked bike → components installed (ever) on that bike.
    private async Task<IReadOnlyCollection<int>> ResolveGearComponentIdsAsync(string? gearId)
    {
        if (string.IsNullOrEmpty(gearId))
        {
            return Array.Empty<int>();
        }

        var link = await _komUoW.GetRepository<IBikeLinkRepository>().GetByExternalIdAsync(ExternalService.Strava, gearId);
        if (link is null)
        {
            return Array.Empty<int>();
        }

        return (await _komUoW.GetRepository<IInstallationRepository>().GetComponentIdsByBikesAsync(new[] { link.BikeId })).ToList();
    }

    private async Task RecalculateSafelyAsync(IReadOnlyCollection<int> componentIds, string context, CancellationToken cancellationToken)
    {
        if (componentIds.Count == 0)
        {
            return;
        }

        try
        {
            await _mediator.Send(new RecalculateComponentsMileageCommand { ComponentIds = componentIds }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Best-effort side effect — recoverable via admin/recalculate-component-mileage. Must not break the mutation or sibling handlers.
            _logger.LogError(ex, "{handler} failed for {context}", nameof(ComponentMileageProjectionUpdater), context);
        }
    }
}
