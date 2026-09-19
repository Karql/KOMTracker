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
    private readonly Services.ComponentMileageService _mileageService;

    public GetComponentInstallationsQueryHandler(IKOMUnitOfWork komUoW, Services.ComponentMileageService mileageService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mileageService = mileageService ?? throw new ArgumentNullException(nameof(mileageService));
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

        var componentsById = (await _komUoW.GetRepository<IComponentRepository>()
            .GetComponentsAsync(request.UserId, includeInactive: true))
            .ToDictionary(c => c.Id);

        foreach (var installation in installations)
        {
            installation.ComponentName = component.Name;
            installation.ComponentCategory = component.Category;
            if (installation.BikeId is int bikeId && bikeNamesById.TryGetValue(bikeId, out var name))
            {
                installation.BikeName = name;
            }
            else if (installation.ParentComponentId is int parentId && componentsById.TryGetValue(parentId, out var parent))
            {
                installation.ParentComponentName = parent.Name;
                installation.ParentComponentCategory = parent.Category;
            }
        }

        // Per-window mileage (Phase 3, live) — one batch for the whole list; Manual rows keep their static totals.
        await _mileageService.ResolveWindowTotalsAsync(installations);

        return installations;
    }
}
