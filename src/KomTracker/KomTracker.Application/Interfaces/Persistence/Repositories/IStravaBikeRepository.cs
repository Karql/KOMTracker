using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IStravaBikeRepository : IRepository
{
    /// <summary>
    /// Mirror the athlete's Strava gear: bulk upsert the provided bikes and delete the athlete's
    /// stored bikes that are no longer returned (removed on Strava).
    /// </summary>
    Task UpsertAthleteBikesAsync(int athleteId, IReadOnlyCollection<StravaBikeEntity> bikes);

    Task<IEnumerable<StravaBikeEntity>> GetByAthleteAsync(int athleteId);

    /// <summary>The stored Strava bike for the athlete, or null (used to validate a link request).</summary>
    Task<StravaBikeEntity?> GetAsync(int athleteId, string gearId);
}
