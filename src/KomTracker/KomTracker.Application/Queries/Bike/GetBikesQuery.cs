using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Queries.Bike;

public class GetBikesQuery : IRequest<IEnumerable<BikeEntity>>
{
    public string UserId { get; set; } = default!;
    public bool IncludeInactive { get; set; }
}

public class GetBikesQueryHandler : IRequestHandler<GetBikesQuery, IEnumerable<BikeEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetBikesQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<BikeEntity>> Handle(GetBikesQuery request, CancellationToken cancellationToken)
    {
        var bikes = (await _komUoW.GetRepository<IBikeRepository>()
            .GetBikesAsync(request.UserId, request.IncludeInactive)).ToList();

        var links = (await _komUoW.GetRepository<IBikeLinkRepository>()
            .GetByBikeIdsAsync(bikes.Select(b => b.Id).ToList()))
            .GroupBy(l => l.BikeId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<BikeLinkEntity>)g.ToList());

        foreach (var bike in bikes)
        {
            if (links.TryGetValue(bike.Id, out var bikeLinks))
            {
                bike.Links = bikeLinks;
            }
        }

        var gearIds = bikes
            .SelectMany(b => b.Links)
            .Where(l => l.ExternalService == ExternalService.Strava)
            .Select(l => l.ExternalId)
            .Distinct()
            .ToList();

        var totalsByGearId = (await _komUoW.GetRepository<IActivityRepository>().GetGearTotalsAsync(gearIds))
            .ToDictionary(x => x.GearId);

        var stravaBikeNameByGearId = (await _komUoW.GetRepository<IStravaBikeRepository>().GetByIdsAsync(gearIds))
            .Where(b => b.Name is not null)
            .ToDictionary(b => b.Id, b => b.Name!);

        foreach (var bike in bikes)
        {
            BikeTotalsCalculator.Apply(bike, totalsByGearId);
            bike.StravaBikeName = ResolveStravaBikeName(bike, stravaBikeNameByGearId);
        }

        return bikes;
    }

    private static string? ResolveStravaBikeName(BikeEntity bike, IReadOnlyDictionary<string, string> namesByGearId)
    {
        foreach (var link in bike.Links)
        {
            if (link.ExternalService == ExternalService.Strava && namesByGearId.TryGetValue(link.ExternalId, out var name))
            {
                return name;
            }
        }

        return null;
    }
}
