using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.ServiceRecord;
using MediatR;

namespace KomTracker.Application.Queries.ServiceRecord;

/// <summary>
/// The caller's services, newest first, with their targets (names resolved, stored mileage at the service date).
/// Optionally only those covering <see cref="BikeId"/> or <see cref="ComponentId"/> (the detail-page panels).
/// </summary>
public class GetServiceRecordsQuery : IRequest<IEnumerable<ServiceRecordEntity>>
{
    public string UserId { get; set; } = default!;
    public int? BikeId { get; set; }
    public int? ComponentId { get; set; }
}

public class GetServiceRecordsQueryHandler : IRequestHandler<GetServiceRecordsQuery, IEnumerable<ServiceRecordEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetServiceRecordsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<ServiceRecordEntity>> Handle(GetServiceRecordsQuery request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IServiceRecordRepository>();

        var services = request.BikeId is int bikeId
            ? await repo.GetByBikeAsync(bikeId)
            : request.ComponentId is int componentId
                ? await repo.GetByComponentAsync(componentId)
                : await repo.GetServiceRecordsAsync(request.UserId);

        // Someone else's bike/component → nothing (looks like "no services").
        var owned = services.Where(s => s.UserId == request.UserId).ToList();

        await ServiceTargetNames.ResolveAsync(_komUoW, request.UserId, owned);

        return owned;
    }
}

/// <summary>Fills each target's read-model name / category from the user's bikes and components (one query each).</summary>
internal static class ServiceTargetNames
{
    public static async Task ResolveAsync(IKOMUnitOfWork komUoW, string userId, IReadOnlyCollection<ServiceRecordEntity> services)
    {
        var targets = services.SelectMany(s => s.Targets).ToList();
        if (targets.Count == 0)
        {
            return;
        }

        var bikes = targets.Any(t => t.BikeId is not null)
            ? (await komUoW.GetRepository<IBikeRepository>().GetBikesAsync(userId, includeInactive: true)).ToDictionary(b => b.Id)
            : new();
        var components = targets.Any(t => t.ComponentId is not null)
            ? (await komUoW.GetRepository<IComponentRepository>().GetComponentsAsync(userId, includeInactive: true)).ToDictionary(c => c.Id)
            : new();

        foreach (var target in targets)
        {
            if (target.BikeId is int bikeId && bikes.TryGetValue(bikeId, out var bike))
            {
                target.TargetName = bike.Name;
            }
            else if (target.ComponentId is int componentId && components.TryGetValue(componentId, out var component))
            {
                target.TargetName = component.Name;
                target.TargetCategory = component.Category;
            }
        }

        // Bikes first, then components, each by name — a stable order for the "Covers" lists.
        foreach (var service in services)
        {
            service.Targets = service.Targets
                .OrderBy(t => t.BikeId is null)
                .ThenBy(t => t.TargetName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
