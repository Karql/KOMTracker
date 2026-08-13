#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFStravaBikeRepository : EFBaseRepository, IStravaBikeRepository
{
    public async Task UpsertAthleteBikesAsync(int athleteId, IReadOnlyCollection<StravaBikeEntity> bikes)
    {
        // Existing ids (reused to stamp audit + detect deletes). Bulk ops bypass the change tracker,
        // so audit is stamped manually here.
        var existingIds = (await _context.StravaBike.AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .Select(x => x.Id)
            .ToListAsync()).ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var bike in bikes)
        {
            if (existingIds.Contains(bike.Id))
            {
                bike.AuditMD = now;
            }
            else
            {
                bike.AuditCD = now;
            }
        }

        await _context.BulkInsertOrUpdateAsync(bikes.ToList(), new BulkConfig
        {
            PropertiesToExcludeOnUpdate = new List<string> { nameof(StravaBikeEntity.AuditCD) },
            PreserveInsertOrder = false,
            SetOutputIdentity = false
        });

        // Delete athlete's bikes Strava no longer returns (deleted there).
        var fetchedIds = bikes.Select(x => x.Id).ToHashSet();
        var idsToDelete = existingIds.Where(id => !fetchedIds.Contains(id)).ToList();

        if (idsToDelete.Count > 0)
        {
            await _context.StravaBike.Where(x => idsToDelete.Contains(x.Id)).ExecuteDeleteAsync();
        }
    }

    public async Task<IEnumerable<StravaBikeEntity>> GetByAthleteAsync(int athleteId)
    {
        return await _context.StravaBike.AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .ToListAsync();
    }

    public Task<StravaBikeEntity?> GetAsync(int athleteId, string gearId)
    {
        return _context.StravaBike.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AthleteId == athleteId && x.Id == gearId);
    }
}
