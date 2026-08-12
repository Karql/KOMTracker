using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IActivityRepository : IRepository
{
    /// <summary>
    /// Bulk upsert an athlete's activities. When <paramref name="deleteFrom"/> is set, also deletes the
    /// athlete's activities with StartDate &gt;= deleteFrom that are NOT in the provided set (window-scoped
    /// delete-detection); when null, deletes across all of the athlete's activities (full sync).
    /// </summary>
    /// <returns>Number of rows deleted (activities removed on Strava within the window).</returns>
    Task<int> UpsertAthleteActivitiesAsync(int athleteId, IReadOnlyCollection<ActivityEntity> activities, DateTime? deleteFrom);

    /// <summary>Total activities currently stored for the athlete (running snapshot for sync-history diagnostics).</summary>
    Task<int> CountAthleteActivitiesAsync(int athleteId);
}
