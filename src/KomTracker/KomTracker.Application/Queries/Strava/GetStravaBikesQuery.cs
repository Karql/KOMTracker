using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Queries.Strava;

/// <summary>The athlete's synced Strava bikes (strava.bike) with their bt.bike link info (if any).</summary>
public class GetStravaBikesQuery : IRequest<IEnumerable<StravaBikeModel>>
{
    public int AthleteId { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetStravaBikesQueryHandler : IRequestHandler<GetStravaBikesQuery, IEnumerable<StravaBikeModel>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetStravaBikesQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<StravaBikeModel>> Handle(GetStravaBikesQuery request, CancellationToken cancellationToken)
    {
        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();
        var bikeLinkRepo = _komUoW.GetRepository<IBikeLinkRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();

        var stravaBikes = (await stravaBikeRepo.GetByAthleteAsync(request.AthleteId)).ToList();
        var gearIds = stravaBikes.Select(x => x.Id).ToList();

        var links = (await bikeLinkRepo.GetByExternalIdsAsync(ExternalService.Strava, gearIds)).ToList();
        var linkByGearId = links.ToDictionary(x => x.ExternalId, x => x.BikeId);

        // Bike names for the "Linked" badge (only the caller's bikes matter for security).
        var bikesById = (await bikeRepo.GetBikesAsync(request.UserId, includeInactive: true))
            .ToDictionary(x => x.Id, x => x.Name);

        return stravaBikes.Select(sb =>
        {
            int? linkedBikeId = linkByGearId.TryGetValue(sb.Id, out var bikeId) && bikesById.ContainsKey(bikeId)
                ? bikeId
                : null;

            return new StravaBikeModel
            {
                Bike = sb,
                LinkedBikeId = linkedBikeId,
                LinkedBikeName = linkedBikeId is int id ? bikesById[id] : null
            };
        }).ToList();
    }
}
