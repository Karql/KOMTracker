using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFActivitySyncHistoryRepository : EFBaseRepository, IActivitySyncHistoryRepository
{
    public void Add(ActivitySyncHistoryEntity entry)
    {
        _context.ActivitySyncHistory.Add(entry);
    }
}
