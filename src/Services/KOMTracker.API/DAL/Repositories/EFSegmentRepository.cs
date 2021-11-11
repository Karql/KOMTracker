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

    public Task<KomsSummaryModel> GetLastKomsSummaryWithEffortsAsync(int athleteId)
    {
        // TODO: Consider split to two queries
        return _context
            .KomsSummary
            .Include(x => x.SegmentEfforts)
            .OrderByDescending(x => x.TrackDate)
            .FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }
}
