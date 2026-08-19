#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
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

    public async Task<IEnumerable<string>> GetDistinctBikeGearIdsAsync(int athleteId)
    {
        // Strava gear ids: bikes "b…", shoes "g…". Filter to bikes so run activities don't drag shoes in.
        return await _context.Activity.AsNoTracking()
            .Where(x => x.AthleteId == athleteId && x.GearId != null && x.GearId.StartsWith("b"))
            .Select(x => x.GearId!)
            .Distinct()
            .ToListAsync();
    }

    public async Task UpsertActivityAsync(ActivityEntity activity)
    {
        // Targeted single-row upsert — NO delete-detection (that belongs to the windowed batch sync). Bulk ops
        // bypass the change tracker, so stamp audit manually: AuditCD on insert, AuditMD on update.
        var exists = await _context.Activity.AsNoTracking().AnyAsync(x => x.Id == activity.Id);

        var now = DateTime.UtcNow;
        if (exists)
        {
            activity.AuditMD = now;
        }
        else
        {
            activity.AuditCD = now;
        }

        await _context.BulkInsertOrUpdateAsync(new List<ActivityEntity> { activity }, new BulkConfig
        {
            PropertiesToExcludeOnUpdate = new List<string> { nameof(ActivityEntity.AuditCD) },
            PreserveInsertOrder = false,
            SetOutputIdentity = false
        });
    }

    public async Task<IEnumerable<ActivityEntity>> GetActivitiesPageAsync(int athleteId, int skip, int take)
    {
        return await _context.Activity.AsNoTracking()
            .Where(x => x.AthleteId == athleteId)
            .OrderByDescending(x => x.StartDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<GearTotalsModel>> GetGearTotalsAsync(IReadOnlyCollection<string> gearIds)
    {
        if (gearIds is null || gearIds.Count == 0)
        {
            return Enumerable.Empty<GearTotalsModel>();
        }

        return await _context.Activity.AsNoTracking()
            .Where(x => x.GearId != null && gearIds.Contains(x.GearId))
            .GroupBy(x => x.GearId!)
            .Select(g => new GearTotalsModel
            {
                GearId = g.Key,
                DistanceMeters = g.Sum(x => x.Distance),
                MovingTimeSeconds = g.Sum(x => (long)x.MovingTime),
                ElevationMeters = g.Sum(x => x.TotalElevationGain),
                ActivityCount = g.Count()
            })
            .ToListAsync();
    }
}
