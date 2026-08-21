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
        var components = (await _komUoW.GetRepository<IComponentRepository>()
            .GetComponentsAsync(request.UserId, request.IncludeInactive)).ToList();

        var warehouseNamesById = (await _komUoW.GetRepository<IWarehouseRepository>()
            .GetWarehousesAsync(request.UserId))
            .ToDictionary(w => w.Id, w => w.Name);

        // Current active installation (where each component is mounted now) — takes display priority over warehouse.
        var activeByComponentId = (await _komUoW.GetRepository<IInstallationRepository>()
            .GetActiveTrackedByComponentsAsync(components.Select(c => c.Id).ToList()))
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.First());

        var bikeNamesById = activeByComponentId.Count == 0
            ? new Dictionary<int, string>()
            : (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(request.UserId, includeInactive: true))
                .ToDictionary(b => b.Id, b => b.Name);

        foreach (var component in components)
        {
            if (component.WarehouseId is int id && warehouseNamesById.TryGetValue(id, out var name))
            {
                component.WarehouseName = name;
            }

            if (activeByComponentId.TryGetValue(component.Id, out var installation))
            {
                component.InstalledOnBikeId = installation.BikeId;
                component.InstalledPosition = installation.Position;
                if (installation.BikeId is int bikeId && bikeNamesById.TryGetValue(bikeId, out var bikeName))
                {
                    component.InstalledOnBikeName = bikeName;
                }
            }
        }

        return components;
    }
}
