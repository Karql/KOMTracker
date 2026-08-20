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

        foreach (var component in components)
        {
            if (component.WarehouseId is int id && warehouseNamesById.TryGetValue(id, out var name))
            {
                component.WarehouseName = name;
            }
        }

        return components;
    }
}
