using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Queries.Component;

public class GetComponentQuery : IRequest<ComponentEntity?>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetComponentQueryHandler : IRequestHandler<GetComponentQuery, ComponentEntity?>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly Services.ComponentMileageService _mileageService;

    public GetComponentQueryHandler(IKOMUnitOfWork komUoW, Services.ComponentMileageService mileageService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mileageService = mileageService ?? throw new ArgumentNullException(nameof(mileageService));
    }

    public async Task<ComponentEntity?> Handle(GetComponentQuery request, CancellationToken cancellationToken)
    {
        var componentRepo = _komUoW.GetRepository<IComponentRepository>();
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();

        var component = await componentRepo.GetComponentAsync(request.Id);

        // Scope to the caller — hide other users' components (looks like "not found").
        if (component is null || component.UserId != request.UserId)
        {
            return null;
        }

        if (component.WarehouseId is int id)
        {
            var warehouse = await _komUoW.GetRepository<IWarehouseRepository>().GetWarehouseAsync(id);
            if (warehouse is not null && warehouse.UserId == request.UserId)
            {
                component.WarehouseName = warehouse.Name;
            }
        }

        // Stored mileage projection (Phase 3).
        var mileage = await _komUoW.GetRepository<IComponentMileageRepository>().GetAsync(component.Id);
        if (mileage is not null)
        {
            component.TotalDistanceKm = mileage.TotalDistanceKm;
            component.TotalMovingHours = mileage.TotalMovingHours;
            component.TotalElevationM = mileage.TotalElevationM;
            component.AttributedActivityCount = mileage.AttributedActivityCount;
        }

        // Current active placements (multi-bike, or a single parent component — D-7).
        var placements = (await installationRepo.GetActiveTrackedInstallationsByComponentAsync(component.Id)).ToList();

        // Immediate children (components installed INTO this one) — current + historical, one level.
        var children = (await installationRepo.GetByParentComponentAsync(component.Id)).ToList();

        // Batch name lookups for bikes + components referenced by placements/children.
        var bikesById = (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(request.UserId, includeInactive: true))
            .ToDictionary(b => b.Id, b => b.Name);
        var componentsById = (await componentRepo.GetComponentsAsync(request.UserId, includeInactive: true))
            .ToDictionary(c => c.Id);

        foreach (var placement in placements)
        {
            if (placement.BikeId is int bId && bikesById.TryGetValue(bId, out var bName))
            {
                placement.BikeName = bName;
            }
            else if (placement.ParentComponentId is int pId && componentsById.TryGetValue(pId, out var parent))
            {
                placement.ParentComponentName = parent.Name;
            }
        }

        component.CurrentPlacements = placements;

        var parentPlacement = placements.FirstOrDefault(p => p.ParentComponentId is not null);
        if (parentPlacement is not null)
        {
            component.ParentComponentId = parentPlacement.ParentComponentId;
            component.ParentComponentName = parentPlacement.ParentComponentName;
            component.InstalledPosition = parentPlacement.Position;
        }
        else
        {
            var bikePlacements = placements.Where(p => p.BikeId is not null).ToList();
            component.InstalledBikeCount = bikePlacements.Select(p => p.BikeId).Distinct().Count();
            var first = bikePlacements.FirstOrDefault();
            if (first is not null)
            {
                component.InstalledOnBikeId = first.BikeId;
                component.InstalledOnBikeName = first.BikeName;
                component.InstalledPosition = first.Position;
            }
        }

        foreach (var child in children)
        {
            if (componentsById.TryGetValue(child.ComponentId, out var childComponent))
            {
                child.ComponentName = childComponent.Name;
                child.ComponentCategory = childComponent.Category;
            }
        }

        // Per-window mileage each child accrued while inside this component (Phase 3, live) — one batch.
        await _mileageService.ResolveWindowTotalsAsync(children);

        component.Children = children;

        return component;
    }
}
