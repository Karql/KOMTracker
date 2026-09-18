using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Queries.Component;

public class GetComponentsQuery : IRequest<IEnumerable<ComponentEntity>>
{
    public string UserId { get; set; } = default!;
    public bool IncludeInactive { get; set; }
}

public class GetComponentsQueryHandler : IRequestHandler<GetComponentsQuery, IEnumerable<ComponentEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetComponentsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<ComponentEntity>> Handle(GetComponentsQuery request, CancellationToken cancellationToken)
    {
        var componentRepo = _komUoW.GetRepository<IComponentRepository>();

        var components = (await componentRepo.GetComponentsAsync(request.UserId, request.IncludeInactive)).ToList();

        var warehouseNamesById = (await _komUoW.GetRepository<IWarehouseRepository>()
            .GetWarehousesAsync(request.UserId))
            .ToDictionary(w => w.Id, w => w.Name);

        // Current active placements per component (multi-bike or a single parent component — D-7).
        var placementsByComponentId = (await _komUoW.GetRepository<IInstallationRepository>()
            .GetActiveTrackedByComponentsAsync(components.Select(c => c.Id).ToList()))
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var bikeNamesById = placementsByComponentId.Count == 0
            ? new Dictionary<int, string>()
            : (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(request.UserId, includeInactive: true))
                .ToDictionary(b => b.Id, b => b.Name);

        var componentNamesById = placementsByComponentId.Count == 0
            ? new Dictionary<int, string>()
            : components.ToDictionary(c => c.Id, c => c.Name);

        foreach (var component in components)
        {
            if (component.WarehouseId is int id && warehouseNamesById.TryGetValue(id, out var name))
            {
                component.WarehouseName = name;
            }

            if (!placementsByComponentId.TryGetValue(component.Id, out var placements))
            {
                continue;
            }

            foreach (var placement in placements)
            {
                if (placement.BikeId is int bId && bikeNamesById.TryGetValue(bId, out var bName))
                {
                    placement.BikeName = bName;
                }
                else if (placement.ParentComponentId is int pId && componentNamesById.TryGetValue(pId, out var pName))
                {
                    placement.ParentComponentName = pName;
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
        }

        return components;
    }
}
