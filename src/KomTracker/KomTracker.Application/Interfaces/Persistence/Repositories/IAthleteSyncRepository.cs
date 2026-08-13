using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IAthleteSyncRepository : IRepository
{
    /// <summary>Athlete ids with activity sync enabled.</summary>
    Task<IEnumerable<int>> GetActivitiesEnabledAthleteIdsAsync();
    Task<AthleteSyncEntity?> GetAsync(int athleteId);

    /// <summary>Upsert the row, setting only <c>ActivitiesEnabled</c> (preserves other capability flags).</summary>
    Task UpsertAsync(AthleteSyncEntity athleteSync);

    /// <summary>Upsert the row, setting only <c>BikesEnabled</c> (preserves <c>ActivitiesEnabled</c>).</summary>
    Task SetBikesEnabledAsync(int athleteId, bool enabled);
}
