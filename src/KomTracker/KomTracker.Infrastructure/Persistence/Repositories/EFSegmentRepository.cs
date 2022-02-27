using KomTracker.Domain.Entities.Segment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using System.Linq;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using System;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFSegmentRepository : EFRepositoryBase<KOMDBContext>, ISegmentRepository
{
    public Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments)
    {
        return _context
            .Segment
            .UpsertRange(segments)
            .WhenMatched(x => new SegmentEntity { }) // No update
            .RunAsync();
    }

    public Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEffots)
    {
        return _context
            .SegmentEffort
            .UpsertRange(segmentEffots)
            .WhenMatched(x => new SegmentEffortEntity { }) // No update
            .RunAsync();
    }

    public async Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return await (
            from ksse in _context.KomsSummarySegmentEffort
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join s in _context.Segment on se.SegmentId equals s.Id
            where ksse.KomSummaryId == _context
                    .KomsSummary
                    .OrderByDescending(x => x.TrackDate)
                    .FirstOrDefault(x => x.AthleteId == athleteId).Id
            select new EffortModel
            {
                SegmentEffort = se,
                SummarySegmentEffort = ksse,
                Segment = s
            }
        ).ToListAsync();
    }

    public async Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(int athleteId, DateTime dateFrom)
    {
        return await (
            from ks in _context.KomsSummary
            join ksse in _context.KomsSummarySegmentEffort.Where(x => x.NewKom == true || x.LostKom == true)
                on ks.Id equals ksse.KomSummaryId
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            join s in _context.Segment on se.SegmentId equals s.Id
            where ks.AthleteId == athleteId && ks.TrackDate >= dateFrom
            select new EffortModel
            {
                SegmentEffort = se,
                SummarySegmentEffort = ksse,
                Segment = s
            }
        ).ToListAsync();
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

    public async Task<IEnumerable<SegmentEntity>> GetSegmentsToRefreshAsync(int top = 100)
    {
        return await _context.Segment
            .OrderByDescending(x => x.AuditMD)
            .Take(top)
            .ToListAsync();
    }
}
