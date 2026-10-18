using KomTracker.Application.Commands.Component;
using KomTracker.Application.Commands.ServiceRecord;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Notifications.Bike;
using KomTracker.Application.Notifications.ServiceRecord;
using KomTracker.Application.Notifications.Strava;
using KomTracker.Domain.Entities.Bike;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KomTracker.Application.Notifications.Component;

/// <summary>
/// The single reactor that keeps the stored mileage projections fresh: the component totals (<c>bt.component_mileage</c>)
/// and the mileage at each service date (<c>bt.service_target</c>, D-SRV-4/5). It subscribes to the domain facts that
/// can change an attribution — <see cref="ComponentChangedNotification"/>, <see cref="InstallationChangedNotification"/>,
/// <see cref="BikeChangedNotification"/>, <see cref="AthleteActivitiesSyncedNotification"/>,
/// <see cref="ActivitySyncedNotification"/>, <see cref="ServiceRecordChangedNotification"/> — resolves each to the
/// affected bikes / components, and recomputes-from-source.
/// <para>
/// The mutation/sync commands only announce what happened; the knowledge of <em>which</em> components that touches
/// (athlete → bikes → components, gear → bike → components) lives here, next to the mileage code, then dispatches the
/// canonical <see cref="RecalculateComponentsMileageCommand"/> and <see cref="RecalculateServiceMileageCommand"/> (which
/// own child expansion + recompute + store).
/// </para>
/// <para>
/// Best-effort: a recompute failure is logged and swallowed so it never breaks the originating mutation or a sibling
/// handler — the projections are always rebuildable via <c>PUT admin/recalculate-component-mileage</c> /
/// <c>PUT admin/recalculate-service-mileage</c>.
/// </para>
/// </summary>
public class ComponentMileageProjectionUpdater :
    INotificationHandler<ComponentChangedNotification>,
    INotificationHandler<InstallationChangedNotification>,
    INotificationHandler<BikeChangedNotification>,
    INotificationHandler<AthleteActivitiesSyncedNotification>,
    INotificationHandler<ActivitySyncedNotification>,
    INotificationHandler<ServiceRecordChangedNotification>
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
        => RecalculateSafelyAsync(Array.Empty<int>(), new[] { notification.ComponentId },
            $"component {notification.ComponentId}", cancellationToken);

    public Task Handle(InstallationChangedNotification notification, CancellationToken cancellationToken)
        => RecalculateSafelyAsync(Array.Empty<int>(), new[] { notification.ComponentId },
            $"installation change on component {notification.ComponentId}", cancellationToken);

    public async Task Handle(BikeChangedNotification notification, CancellationToken cancellationToken)
    {
        var bikeIds = new[] { notification.BikeId };
        await RecalculateSafelyAsync(bikeIds, await ComponentIdsOnBikesAsync(bikeIds), $"bike {notification.BikeId}", cancellationToken);
    }

    public async Task Handle(AthleteActivitiesSyncedNotification notification, CancellationToken cancellationToken)
    {
        var bikeIds = await ResolveAthleteBikeIdsAsync(notification.AthleteId);
        await RecalculateSafelyAsync(bikeIds, await ComponentIdsOnBikesAsync(bikeIds),
            $"athlete {notification.AthleteId} activities sync", cancellationToken);
    }

    public async Task Handle(ActivitySyncedNotification notification, CancellationToken cancellationToken)
    {
        // The ride's current bike, plus the bike it was on before when it moved (that one has to drop the ride).
        var bikeIds = await ResolveGearBikeIdsAsync(notification.GearId, notification.PreviousGearId);
        await RecalculateSafelyAsync(bikeIds, await ComponentIdsOnBikesAsync(bikeIds),
            $"activity {notification.ActivityId} sync (gear {notification.GearId}, previous gear {notification.PreviousGearId})", cancellationToken);
    }

    public async Task Handle(ServiceRecordChangedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new RecalculateServiceMileageCommand { ServiceIds = new[] { notification.ServiceId } }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{handler} failed for service {serviceId}", nameof(ComponentMileageProjectionUpdater), notification.ServiceId);
        }
    }

    // Athlete → owning user → their bikes (incl. inactive).
    private async Task<IReadOnlyCollection<int>> ResolveAthleteBikeIdsAsync(int athleteId)
    {
        var userId = (await _userService.GetUserAsync(athleteId))?.Id;
        if (userId is null)
        {
            return Array.Empty<int>();
        }

        return (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(userId, includeInactive: true))
            .Select(b => b.Id)
            .ToList();
    }

    // Gear(s) → linked bike(s). Empty/unlinked gears are skipped.
    private async Task<IReadOnlyCollection<int>> ResolveGearBikeIdsAsync(params string?[] gearIds)
    {
        var linkRepo = _komUoW.GetRepository<IBikeLinkRepository>();
        var bikeIds = new List<int>();

        foreach (var gearId in gearIds.Where(g => !string.IsNullOrEmpty(g)).Distinct())
        {
            var link = await linkRepo.GetByExternalIdAsync(ExternalService.Strava, gearId!);
            if (link is not null)
            {
                bikeIds.Add(link.BikeId);
            }
        }

        return bikeIds.Distinct().ToList();
    }

    // Bikes → components installed (ever) on them.
    private async Task<IReadOnlyCollection<int>> ComponentIdsOnBikesAsync(IReadOnlyCollection<int> bikeIds)
    {
        if (bikeIds.Count == 0)
        {
            return Array.Empty<int>();
        }

        return (await _komUoW.GetRepository<IInstallationRepository>().GetComponentIdsByBikesAsync(bikeIds)).ToList();
    }

    private async Task RecalculateSafelyAsync(IReadOnlyCollection<int> bikeIds, IReadOnlyCollection<int> componentIds,
        string context, CancellationToken cancellationToken)
    {
        if (componentIds.Count > 0)
        {
            try
            {
                await _mediator.Send(new RecalculateComponentsMileageCommand { ComponentIds = componentIds }, cancellationToken);
            }
            catch (Exception ex)
            {
                // Best-effort side effect — recoverable via admin/recalculate-component-mileage. Must not break the mutation or sibling handlers.
                _logger.LogError(ex, "{handler} failed (component mileage) for {context}", nameof(ComponentMileageProjectionUpdater), context);
            }
        }

        if (bikeIds.Count > 0 || componentIds.Count > 0)
        {
            try
            {
                await _mediator.Send(new RecalculateServiceMileageCommand { BikeIds = bikeIds, ComponentIds = componentIds }, cancellationToken);
            }
            catch (Exception ex)
            {
                // Best-effort — recoverable via admin/recalculate-service-mileage.
                _logger.LogError(ex, "{handler} failed (service mileage) for {context}", nameof(ComponentMileageProjectionUpdater), context);
            }
        }
    }
}
