using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Queries.Bike;

public class GetBikeQuery : IRequest<BikeEntity?>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetBikeQueryHandler : IRequestHandler<GetBikeQuery, BikeEntity?>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetBikeQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<BikeEntity?> Handle(GetBikeQuery request, CancellationToken cancellationToken)
    {
        var bike = await _komUoW.GetRepository<IBikeRepository>().GetBikeAsync(request.Id);

        // Scope to the caller — hide other users' bikes (looks like "not found").
        if (bike is null || bike.UserId != request.UserId)
        {
            return null;
        }

        bike.Links = (await _komUoW.GetRepository<IBikeLinkRepository>()
            .GetByBikeIdsAsync(new[] { bike.Id })).ToList();

        var gearIds = bike.Links
            .Where(l => l.ExternalService == ExternalService.Strava)
            .Select(l => l.ExternalId)
            .Distinct()
            .ToList();

        var totalsByGearId = (await _komUoW.GetRepository<IActivityRepository>().GetGearTotalsAsync(gearIds))
            .ToDictionary(x => x.GearId);

        BikeTotalsCalculator.Apply(bike, totalsByGearId);

        var stravaBike = (await _komUoW.GetRepository<IStravaBikeRepository>().GetByIdsAsync(gearIds))
            .FirstOrDefault(b => b.Name is not null);
        bike.StravaBikeName = stravaBike?.Name;

        return bike;
    }
}
