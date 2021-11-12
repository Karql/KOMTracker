using KOMTracker.API.Models.Segment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using FlexLabs.EntityFrameworkCore.Upsert;
using System.Linq;

namespace KOMTracker.API.DAL.Repositories;

public class EFSegmentRepository : EFRepositoryBase<KOMDBContext>, ISegmentRepository
{
    public Task AddSegmentsIfNotExists(IEnumerable<SegmentModel> segments)
    {
        return _context
            .Segment
            .UpsertRange(segments)
            .WhenMatched(x => new SegmentModel { }) // No update
            .RunAsync();
    }

    public async Task<IEnumerable<SegmentEffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return await _context.SegmentEffort
            .Where(x => x.KomSummaries.Contains(_context
                .KomsSummary
                .OrderByDescending(x => x.TrackDate)
                .FirstOrDefault(x => x.AthleteId == athleteId))
            )
            .ToArrayAsync();
    }

    public async Task AddKomsSummaryWithEffortsAsync(KomsSummaryModel komsSummary)
    {
        await _context.KomsSummary.AddAsync(komsSummary);
    }
}
