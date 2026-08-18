using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFActivitySyncHistoryRepository : EFBaseRepository, IActivitySyncHistoryRepository
{
    public void Add(ActivitySyncHistoryEntity entry)
    {
        _context.ActivitySyncHistory.Add(entry);
    }

    public Task<bool> AnyForAthleteAsync(int athleteId)
    {
        return _context.ActivitySyncHistory.AsNoTracking().AnyAsync(x => x.AthleteId == athleteId);
    }

    public async Task<IEnumerable<ActivitySyncHistoryEntity>> GetRecentByAthleteAsync(int athleteId, int take)
    {
        return await _context.ActivitySyncHistory.AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .OrderByDescending(x => x.RunAt)
            .Take(take)
            .ToListAsync();
    }
}
