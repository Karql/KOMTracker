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

    public GetComponentQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<ComponentEntity?> Handle(GetComponentQuery request, CancellationToken cancellationToken)
    {
        var component = await _komUoW.GetRepository<IComponentRepository>().GetComponentAsync(request.Id);

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

        return component;
    }
}
