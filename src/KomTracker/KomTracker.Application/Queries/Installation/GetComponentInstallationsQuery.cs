using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Queries.Installation;

/// <summary>A component's installations (current first), with bike names resolved. Empty if not owned.</summary>
public class GetComponentInstallationsQuery : IRequest<IEnumerable<InstallationEntity>>
{
    public int ComponentId { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetComponentInstallationsQueryHandler : IRequestHandler<GetComponentInstallationsQuery, IEnumerable<InstallationEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetComponentInstallationsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<InstallationEntity>> Handle(GetComponentInstallationsQuery request, CancellationToken cancellationToken)
    {
        var component = await _komUoW.GetRepository<IComponentRepository>().GetComponentAsync(request.ComponentId);
        if (component is null || component.UserId != request.UserId)
        {
            return Enumerable.Empty<InstallationEntity>();
        }

        var installations = (await _komUoW.GetRepository<IInstallationRepository>()
            .GetByComponentAsync(request.ComponentId)).ToList();

        var bikeNamesById = (await _komUoW.GetRepository<IBikeRepository>()
            .GetBikesAsync(request.UserId, includeInactive: true))
            .ToDictionary(b => b.Id, b => b.Name);

        foreach (var installation in installations)
        {
            installation.ComponentName = component.Name;
            installation.ComponentCategory = component.Category;
            if (installation.BikeId is int bikeId && bikeNamesById.TryGetValue(bikeId, out var name))
            {
                installation.BikeName = name;
            }
        }

        return installations;
    }
}
