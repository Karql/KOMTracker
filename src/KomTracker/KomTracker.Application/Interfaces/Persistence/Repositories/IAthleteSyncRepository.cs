using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IAthleteSyncRepository : IRepository
{
    /// <summary>Athlete ids with activity sync enabled.</summary>
    Task<IEnumerable<int>> GetActivitiesEnabledAthleteIdsAsync();
    Task<AthleteSyncEntity?> GetAsync(int athleteId);
    Task UpsertAsync(AthleteSyncEntity athleteSync);
}
