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

public class EFActivityRepository : EFBaseRepository, IActivityRepository
{
    public async Task<int> UpsertAthleteActivitiesAsync(int athleteId, IReadOnlyCollection<ActivityEntity> activities, DateTime? deleteFrom)
    {
        // Existing ids in the sync window (reused to stamp audit + detect deletes). Bulk ops bypass the
        // change tracker, so audit is stamped manually here.
        var existingQuery = _context.Activity.AsNoTracking().Where(x => x.AthleteId == athleteId);
        if (deleteFrom.HasValue)
        {
            existingQuery = existingQuery.Where(x => x.StartDate >= deleteFrom.Value);
        }

        var existingIds = (await existingQuery.Select(x => x.Id).ToListAsync()).ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var activity in activities)
        {
            if (existingIds.Contains(activity.Id))
            {
                activity.AuditMD = now;
            }
            else
            {
                activity.AuditCD = now;
            }
        }

        await _context.BulkInsertOrUpdateAsync(activities.ToList(), new BulkConfig
        {
            PropertiesToExcludeOnUpdate = new List<string> { nameof(ActivityEntity.AuditCD) },
            PreserveInsertOrder = false,
            SetOutputIdentity = false
        });

        // Delete window rows that Strava no longer returns (deleted there).
        var fetchedIds = activities.Select(x => x.Id).ToHashSet();
        var idsToDelete = existingIds.Where(id => !fetchedIds.Contains(id)).ToList();

        if (idsToDelete.Count > 0)
        {
            await _context.Activity.Where(x => idsToDelete.Contains(x.Id)).ExecuteDeleteAsync();
        }

        return idsToDelete.Count;
    }

    public Task<int> CountAthleteActivitiesAsync(int athleteId)
        => _context.Activity.AsNoTracking().CountAsync(x => x.AthleteId == athleteId);
}
