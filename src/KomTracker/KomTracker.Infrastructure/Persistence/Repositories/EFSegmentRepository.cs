using KomTracker.Domain.Entities.Segment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using System.Linq;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using System;
using EFCore.BulkExtensions;
using static MoreLinq.Extensions.ForEachExtension;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFSegmentRepository : EFBaseRepository, ISegmentRepository
{
    public Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments)
    {
        SetAuidtCD(segments);

        return _context
            .Segment
            .UpsertRange(segments)
            .WhenMatched(x => new SegmentEntity { }) // No update
            .RunAsync();
    }

    public Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEffots)
    {
        SetAuidtCD(segmentEffots);

        return _context
            .SegmentEffort
            .UpsertRange(segmentEffots)
            .WhenMatched(x => new SegmentEffortEntity { }) // No update
            .RunAsync();
    }

    public async Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        // to distinguish between null or zero
        var ks = await _context
                    .KomsSummary
                    .OrderByDescending(x => x.TrackDate)
                    .FirstOrDefaultAsync(x => x.AthleteId == athleteId);

        if (ks == null)
        {
            return null;
        }

        return await (
            from ksse in _context.KomsSummarySegmentEffort
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join s in _context.Segment on se.SegmentId equals s.Id
            where ksse.KomSummaryId == ks.Id
            select new EffortModel
            {
                SegmentEffort = se,
                SummarySegmentEffort = ksse,
                Segment = s
            }
        ).ToListAsync();
    }

    public async Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(IEnumerable<int> athleteIds, DateTime? dateFrom, int? top = null)
    {
        var query =
            from ks in _context.KomsSummary
            join ksse in _context.KomsSummarySegmentEffort.Where(x => x.NewKom == true
                || x.LostKom == true
                || x.ImprovedKom == true
                || x.ReturnedKom == true)
                on ks.Id equals ksse.KomSummaryId
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join s in _context.Segment on se.SegmentId equals s.Id
            where athleteIds.Contains(ks.AthleteId)
                && (!dateFrom.HasValue || ks.TrackDate >= dateFrom)
            orderby ksse.AuditCD descending
            select new EffortModel
            {
                SegmentEffort = se,
                SummarySegmentEffort = ksse,
                Segment = s
            };

        if (top.HasValue)
        {
            query = query.Take(top.Value);
        }
        
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<KomsSummaryEntity>> GetKomsSummariesAsync(int athleteId, DateTime dateFrom)
    {
        return await _context.KomsSummary
            .Where(x => x.AthleteId == athleteId && x.TrackDate >= dateFrom)
            .ToListAsync();
    }

    public async Task AddKomsSummaryAsync(KomsSummaryEntity komsSummary)
    {
        await _context.KomsSummary.AddAsync(komsSummary);
    }

    public async Task AddKomsSummariesSegmentEffortsAsync(IEnumerable<KomsSummarySegmentEffortEntity> komsSummariesSegmentEfforts)
    {
        await _context.KomsSummarySegmentEffort.AddRangeAsync(komsSummariesSegmentEfforts);
    }

    public async Task<IEnumerable<SegmentEntity>> GetSegmentsToRefreshAsync(int top = 100, TimeSpan? minTimeFromLastRefresh = null)
    {
        minTimeFromLastRefresh ??= TimeSpan.FromHours(24);
        var maxAuditMD = DateTime.UtcNow - minTimeFromLastRefresh;

        return await _context.Segment
            .Where(x => !x.AuditMD.HasValue || x.AuditMD < maxAuditMD)
            .OrderBy(x => x.AuditMD.HasValue) // first never queried
            .ThenBy(x => x.AuditMD)           // then oldest
            .Take(top)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task UpdateSegmentsAsync(IEnumerable<SegmentEntity> segments)
    {
        SetAuidtMD(segments);

        // always update full list for ability to query last updated
        return _context.BulkUpdateAsync(segments.ToList(), new BulkConfig
        {
            CalculateStats = false,
            PreserveInsertOrder = false,
            TrackingEntities = false,
            PropertiesToIncludeOnUpdate = new List<string>
            {
                nameof(SegmentEntity.AuditMD),
                nameof(SegmentEntity.CreatedAt),
                nameof(SegmentEntity.UpdatedAt),
                nameof(SegmentEntity.TotalElevationGain),
                nameof(SegmentEntity.EffortCount),
                nameof(SegmentEntity.AthleteCount),
                nameof(SegmentEntity.StarCount),
                nameof(SegmentEntity.MapPolyline)
            },           
        });
    }

    public async Task<IEnumerable<SegmentEffortEntity>> GetSegmentEffortsAsync(HashSet<long> ids)
    {
        return await _context.SegmentEffort.Where(x => ids.Contains(x.Id)).ToArrayAsync();
    }
}
