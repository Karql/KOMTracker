using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IActivitySyncHistoryRepository : IRepository
{
    /// <summary>Stage a sync-run history row (committed by the unit of work).</summary>
    void Add(ActivitySyncHistoryEntity entry);

    /// <summary>Whether the athlete has any recorded sync run (gates the one-time first-enable backfill).</summary>
    Task<bool> AnyForAthleteAsync(int athleteId);

    /// <summary>Most recent sync runs for the athlete, newest first.</summary>
    Task<IEnumerable<ActivitySyncHistoryEntity>> GetRecentByAthleteAsync(int athleteId, int take);
}
