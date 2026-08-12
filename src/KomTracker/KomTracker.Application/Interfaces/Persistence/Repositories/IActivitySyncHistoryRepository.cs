using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IActivitySyncHistoryRepository : IRepository
{
    /// <summary>Stage a sync-run history row (committed by the unit of work).</summary>
    void Add(ActivitySyncHistoryEntity entry);
}
