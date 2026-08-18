using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IAthleteSyncRepository : IRepository
{
    /// <summary>Athlete ids with activity auto-sync enabled.</summary>
    Task<IEnumerable<int>> GetActivitiesEnabledAthleteIdsAsync();

    /// <summary>Athlete ids with bike auto-sync enabled.</summary>
    Task<IEnumerable<int>> GetBikesEnabledAthleteIdsAsync();

    Task<AthleteSyncEntity?> GetAsync(int athleteId);

    /// <summary>Upsert the row, setting only <c>ActivitiesEnabled</c> (preserves <c>BikesEnabled</c>).</summary>
    Task SetActivitiesEnabledAsync(int athleteId, bool enabled);

    /// <summary>Upsert the row, setting only <c>BikesEnabled</c> (preserves <c>ActivitiesEnabled</c>).</summary>
    Task SetBikesEnabledAsync(int athleteId, bool enabled);
}
