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
            // Order by KomSummaryId (not AuditCD) on purpose: the composite PK
            // (koms_summary_id, segment_effort_id) lets Postgres do a backward index
            // scan and stop early at the LIMIT, while AuditCD has no index and forces a
            // full sort. koms_summary_id is monotonic with AuditCD (summaries get a
            // serial id and AuditCD is stamped at creation), so this yields the same
            // most-recent rows. Final display order is set in-memory by the handler.
            orderby ksse.KomSummaryId descending
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

    #region KOM takeovers (battle field)

    public async Task<IEnumerable<KomTakeoverChangeModel>> GetSummaryChangesForTakeoverAsync(int komsSummaryId)
    {
        var rows = await (
            from ks in _context.KomsSummary
            join ksse in _context.KomsSummarySegmentEffort on ks.Id equals ksse.KomSummaryId
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join a in _context.Athlete on ks.AthleteId equals a.AthleteId
            where ks.Id == komsSummaryId
                && (ksse.NewKom || ksse.LostKom || ksse.ReturnedKom)
            select new { ks.AthleteId, a.Sex, se.SegmentId, ksse.SegmentEffortId, KomsSummaryId = ks.Id, ks.TrackDate, ksse.NewKom, ksse.LostKom }
        ).AsNoTracking().ToListAsync();

        return rows.Select(r => new KomTakeoverChangeModel
        {
            AthleteId = r.AthleteId,
            Sex = r.Sex,
            SegmentId = r.SegmentId,
            SegmentEffortId = r.SegmentEffortId,
            KomsSummaryId = r.KomsSummaryId,
            TrackDate = r.TrackDate,
            ChangeType = r.NewKom ? KomChangeTypeEnum.New : r.LostKom ? KomChangeTypeEnum.Lost : KomChangeTypeEnum.Returned
        }).ToList();
    }

    public async Task<IEnumerable<KomTakeoverChangeModel>> GetCounterpartChangesAsync(int komsSummaryId, IEnumerable<long> segmentIds, TimeSpan window, int excludeAthleteId)
    {
        var segmentIdsList = segmentIds?.ToList() ?? new List<long>();
        if (segmentIdsList.Count == 0)
        {
            return Enumerable.Empty<KomTakeoverChangeModel>();
        }

        var refSummary = await _context.KomsSummary.AsNoTracking().FirstOrDefaultAsync(x => x.Id == komsSummaryId);
        if (refSummary == null)
        {
            return Enumerable.Empty<KomTakeoverChangeModel>();
        }

        var minTrackDate = refSummary.TrackDate - window;

        var rows = await (
            from ks in _context.KomsSummary
            join ksse in _context.KomsSummarySegmentEffort on ks.Id equals ksse.KomSummaryId
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join a in _context.Athlete on ks.AthleteId equals a.AthleteId
            where ks.Id < komsSummaryId
                && ks.AthleteId != excludeAthleteId
                && ks.TrackDate >= minTrackDate
                && segmentIdsList.Contains(se.SegmentId)
                && (ksse.NewKom || ksse.LostKom)
            select new { ks.AthleteId, a.Sex, se.SegmentId, ksse.SegmentEffortId, KomsSummaryId = ks.Id, ks.TrackDate, ksse.NewKom, ksse.LostKom }
        ).AsNoTracking().ToListAsync();

        return rows.Select(r => new KomTakeoverChangeModel
        {
            AthleteId = r.AthleteId,
            Sex = r.Sex,
            SegmentId = r.SegmentId,
            SegmentEffortId = r.SegmentEffortId,
            KomsSummaryId = r.KomsSummaryId,
            TrackDate = r.TrackDate,
            ChangeType = r.NewKom ? KomChangeTypeEnum.New : KomChangeTypeEnum.Lost
        }).ToList();
    }

    public async Task<IEnumerable<KomTakeoverEntity>> GetActiveTakeoversByLostEffortAsync(IEnumerable<long> lostSegmentEffortIds)
    {
        var ids = lostSegmentEffortIds?.ToList() ?? new List<long>();
        if (ids.Count == 0)
        {
            return Enumerable.Empty<KomTakeoverEntity>();
        }

        // Tracked on purpose - caller flips Reverted and persists via SaveChangesAsync (sets audit_md).
        return await _context.KomTakeover
            .Where(x => !x.Reverted && ids.Contains(x.LostSegmentEffortId))
            .ToListAsync();
    }

    public Task AddTakeoversIfNotExistsAsync(IEnumerable<KomTakeoverEntity> takeovers)
    {
        var list = takeovers?.ToList() ?? new List<KomTakeoverEntity>();
        if (list.Count == 0)
        {
            return Task.CompletedTask;
        }

        SetAuidtCD(list);

        return _context
            .KomTakeover
            .UpsertRange(list)
            .On(x => x.TakenSegmentEffortId)
            .WhenMatched(x => new KomTakeoverEntity { }) // No update
            .RunAsync();
    }

    #endregion
}
