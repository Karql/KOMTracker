using KomTracker.Domain.Entities.Segment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using System.Linq;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;

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

    public async Task<IEnumerable<SegmentEffortWithLinkToKomsSummaryModel>> GetLastKomsSummaryEffortsWithLinksAsync(int athleteId)
    {
        return await (
            from ksse in _context.KomsSummarySegmentEffort
            join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
            where ksse.KomSummaryId == _context
                    .KomsSummary
                    .OrderByDescending(x => x.TrackDate)
                    .FirstOrDefault(x => x.AthleteId == athleteId).Id
            select new SegmentEffortWithLinkToKomsSummaryModel
            {
                SegmentEffort = se,
                Link = ksse
            }
        ).ToListAsync();
    }

    public async Task AddKomsSummaryAsync(KomsSummaryEntity komsSummary)
    {
        await _context.KomsSummary.AddAsync(komsSummary);
    }

    public async Task AddKomsSummariesSegmentEffortsAsync(IEnumerable<KomsSummarySegmentEffortEntity> komsSummariesSegmentEfforts)
    {
        await _context.KomsSummarySegmentEffort.AddRangeAsync(komsSummariesSegmentEfforts);
    }
}
