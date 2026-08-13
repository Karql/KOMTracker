using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
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

        return bikes;
    }
}
