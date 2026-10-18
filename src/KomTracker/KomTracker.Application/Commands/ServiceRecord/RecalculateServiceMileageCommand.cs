using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.ServiceRecord;
using MediatR;

namespace KomTracker.Application.Commands.ServiceRecord;

/// <summary>
/// The canonical recompute-from-source of the stored "mileage at the service date" on <c>bt.service_target</c>
/// (D-SRV-4): every target of <see cref="ServiceIds"/>, every target pointing at <see cref="ComponentIds"/> (and their
/// children — current AND historical, since a parent's window change alters a nested child's attribution) or at
/// <see cref="BikeIds"/>. <see cref="All"/> rebuilds every target (admin). Idempotent. Dispatched by the admin endpoint
/// and by <see cref="Notifications.Component.ComponentMileageProjectionUpdater"/>.
/// </summary>
public class RecalculateServiceMileageCommand : IRequest<Result>
{
    public IReadOnlyCollection<int> ServiceIds { get; set; } = Array.Empty<int>();
    public IReadOnlyCollection<int> ComponentIds { get; set; } = Array.Empty<int>();
    public IReadOnlyCollection<int> BikeIds { get; set; } = Array.Empty<int>();
    public bool All { get; set; }
}

public class RecalculateServiceMileageCommandHandler : IRequestHandler<RecalculateServiceMileageCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ComponentMileageService _mileageService;

    public RecalculateServiceMileageCommandHandler(IKOMUnitOfWork komUoW, ComponentMileageService mileageService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mileageService = mileageService ?? throw new ArgumentNullException(nameof(mileageService));
    }

    public async Task<Result> Handle(RecalculateServiceMileageCommand request, CancellationToken cancellationToken)
    {
        var serviceRepo = _komUoW.GetRepository<IServiceRecordRepository>();

        IReadOnlyList<ServiceTargetEntity> targets;
        if (request.All)
        {
            targets = await serviceRepo.GetAllTargetsAsync();
        }
        else
        {
            var componentIds = request.ComponentIds;
            if (componentIds.Count > 0)
            {
                var childIds = await _komUoW.GetRepository<IInstallationRepository>().GetChildComponentIdsByParentsAsync(componentIds);
                componentIds = componentIds.Concat(childIds).Distinct().ToList();
            }

            if (request.ServiceIds.Count == 0 && componentIds.Count == 0 && request.BikeIds.Count == 0)
            {
                return Result.Ok();
            }

            targets = await serviceRepo.GetTargetsForRecalculationAsync(request.ServiceIds, componentIds, request.BikeIds);
        }

        if (targets.Count == 0)
        {
            return Result.Ok();
        }

        // One batch for all component points and one for all bike points, then in-memory windowing.
        var componentTotals = await _mileageService.ComputeTotalsAtAsync(targets
            .Where(t => t.ComponentId is not null)
            .Select(t => (t.ComponentId!.Value, t.Service.Date))
            .ToList());
        var bikeTotals = await _mileageService.ComputeBikeTotalsAtAsync(targets
            .Where(t => t.BikeId is not null)
            .Select(t => (t.BikeId!.Value, t.Service.Date))
            .ToList());

        var now = DateTime.UtcNow;
        foreach (var target in targets)
        {
            ComponentMileageService.Totals? totals = null;
            if (target.ComponentId is int componentId && componentTotals.TryGetValue((componentId, target.Service.Date), out var c))
            {
                totals = c;
            }
            else if (target.BikeId is int bikeId && bikeTotals.TryGetValue((bikeId, target.Service.Date), out var b))
            {
                totals = b;
            }

            target.DistanceKm = totals?.DistanceKm;
            target.MovingHours = totals?.MovingHours;
            target.ElevationM = totals?.ElevationM;
            target.ComputedAt = now;
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}
