#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFAthleteSyncRepository : EFBaseRepository, IAthleteSyncRepository
{
    public async Task<IEnumerable<int>> GetActivitiesEnabledAthleteIdsAsync()
    {
        return await _context.AthleteSync
            .Where(x => x.ActivitiesEnabled)
            .Select(x => x.AthleteId)
            .ToListAsync();
    }

    public Task<AthleteSyncEntity?> GetAsync(int athleteId)
    {
        return _context.AthleteSync.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }

    public Task UpsertAsync(AthleteSyncEntity athleteSync)
    {
        athleteSync.AuditCD = DateTime.UtcNow;

        return _context.AthleteSync
            .Upsert(athleteSync)
            .WhenMatched((db, model) => new AthleteSyncEntity
            {
                AuditMD = DateTime.UtcNow,
                ActivitiesEnabled = model.ActivitiesEnabled
            })
            .RunAsync();
    }

    public Task SetBikesEnabledAsync(int athleteId, bool enabled)
    {
        var athleteSync = new AthleteSyncEntity
        {
            AthleteId = athleteId,
            BikesEnabled = enabled,
            AuditCD = DateTime.UtcNow
        };

        return _context.AthleteSync
            .Upsert(athleteSync)
            .WhenMatched((db, model) => new AthleteSyncEntity
            {
                AuditMD = DateTime.UtcNow,
                BikesEnabled = model.BikesEnabled
            })
            .RunAsync();
    }
}
