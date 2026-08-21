using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Queries.Installation;

/// <summary>Installations on a bike (current first), with component name/category resolved. Empty if not owned.</summary>
public class GetBikeInstallationsQuery : IRequest<IEnumerable<InstallationEntity>>
{
    public int BikeId { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetBikeInstallationsQueryHandler : IRequestHandler<GetBikeInstallationsQuery, IEnumerable<InstallationEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetBikeInstallationsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<InstallationEntity>> Handle(GetBikeInstallationsQuery request, CancellationToken cancellationToken)
    {
        var bike = await _komUoW.GetRepository<IBikeRepository>().GetBikeAsync(request.BikeId);
        if (bike is null || bike.UserId != request.UserId)
        {
            return Enumerable.Empty<InstallationEntity>();
        }

        var installations = (await _komUoW.GetRepository<IInstallationRepository>()
            .GetByBikeAsync(request.BikeId)).ToList();

        var componentsById = (await _komUoW.GetRepository<IComponentRepository>()
            .GetComponentsAsync(request.UserId, includeInactive: true))
            .ToDictionary(c => c.Id);

        foreach (var installation in installations)
        {
            installation.BikeName = bike.Name;
            if (componentsById.TryGetValue(installation.ComponentId, out var component))
            {
                installation.ComponentName = component.Name;
                installation.ComponentCategory = component.Category;
            }
        }

        return installations;
    }
}
