using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Warehouse;
using MediatR;

namespace KomTracker.Application.Queries.Warehouse;

public class GetWarehousesQuery : IRequest<IEnumerable<WarehouseEntity>>
{
    public string UserId { get; set; } = default!;
}

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IEnumerable<WarehouseEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetWarehousesQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<WarehouseEntity>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        return await _komUoW.GetRepository<IWarehouseRepository>().GetWarehousesAsync(request.UserId);
    }
}
