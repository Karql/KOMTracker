using KomTracker.Domain.Entities.Segment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using System.Linq;
using KomTracker.Application.Interfaces.Persistence.Repositories;

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

    public async Task<IEnumerable<SegmentEffortEntity>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return await _context.SegmentEffort
            .Where(x => x.KomSummaries.Contains(_context
                .KomsSummary
                .OrderByDescending(x => x.TrackDate)
                .FirstOrDefault(x => x.AthleteId == athleteId))
            )
            .ToArrayAsync();
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
