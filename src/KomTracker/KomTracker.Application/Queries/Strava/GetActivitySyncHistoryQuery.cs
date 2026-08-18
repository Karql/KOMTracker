using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using MediatR;

namespace KomTracker.Application.Queries.Strava;

/// <summary>Recent activity-sync runs for the athlete (newest first) — for the "last updated" history dialog.</summary>
public class GetActivitySyncHistoryQuery : IRequest<IEnumerable<ActivitySyncHistoryEntity>>
{
    public int AthleteId { get; set; }
    public int Take { get; set; } = 20;
}

public class GetActivitySyncHistoryQueryHandler : IRequestHandler<GetActivitySyncHistoryQuery, IEnumerable<ActivitySyncHistoryEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetActivitySyncHistoryQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public Task<IEnumerable<ActivitySyncHistoryEntity>> Handle(GetActivitySyncHistoryQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take <= 0 ? 20 : request.Take;
        return _komUoW.GetRepository<IActivitySyncHistoryRepository>().GetRecentByAthleteAsync(request.AthleteId, take);
    }
}
