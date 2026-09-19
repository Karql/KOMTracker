using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Component;

/// <summary>
/// The canonical recompute-from-source operation for the stored mileage projection (bt.component_mileage): recomputes
/// the given components <em>and their children</em> (current AND historical — a parent's window change alters a
/// since-removed nested child's attribution) and upserts. Idempotent. Dispatched by the admin backfill endpoint and by
/// <see cref="Notifications.Component.ComponentMileageProjectionUpdater"/> as it reacts to mutation/sync events.
/// </summary>
public class RecalculateComponentsMileageCommand : IRequest<Result>
{
    public IReadOnlyCollection<int> ComponentIds { get; set; } = Array.Empty<int>();
}

public class RecalculateComponentsMileageCommandHandler : IRequestHandler<RecalculateComponentsMileageCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ComponentMileageService _mileageService;

    public RecalculateComponentsMileageCommandHandler(IKOMUnitOfWork komUoW, ComponentMileageService mileageService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mileageService = mileageService ?? throw new ArgumentNullException(nameof(mileageService));
    }

    public async Task<Result> Handle(RecalculateComponentsMileageCommand request, CancellationToken cancellationToken)
    {
        if (request.ComponentIds is null || request.ComponentIds.Count == 0)
        {
            return Result.Ok();
        }

        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();
        var mileageRepo = _komUoW.GetRepository<IComponentMileageRepository>();

        // Expand to include children — current AND historical (a parent's window change alters a nested child's
        // attribution even for a window when the child has since been removed, e.g. new private rides sync in later).
        var childIds = await installationRepo.GetChildComponentIdsByParentsAsync(request.ComponentIds);
        var componentIds = request.ComponentIds
            .Concat(childIds)
            .Distinct()
            .ToList();

        // One batch of queries, then in-memory windowing (no per-component / per-window round-trips).
        var totalsByComponent = await _mileageService.ComputeTotalsAsync(componentIds);
        var now = DateTime.UtcNow;

        foreach (var (componentId, totals) in totalsByComponent)
        {
            await mileageRepo.UpsertAsync(new ComponentMileageEntity
            {
                ComponentId = componentId,
                TotalDistanceKm = totals.DistanceKm,
                TotalMovingHours = totals.MovingHours,
                TotalElevationM = totals.ElevationM,
                AttributedActivityCount = totals.ActivityCount,
                ComputedAt = now
            });
        }

        return Result.Ok();
    }
}
