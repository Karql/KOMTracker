using KOMTracker.API.Models.Segment;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KOMTracker.API.DAL.Repositories;

public interface ISegmentRepository : IRepository
{
    Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentModel> segments);

    Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortModel> segmentEffots);

    Task<IEnumerable<SegmentEffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId);

    Task AddKomsSummaryAsync(KomsSummaryModel komsSummary);

    Task AddKomsSummariesSegmentEffortsAsync(IEnumerable<KomsSummarySegmentEffortModel> komsSummariesSegmentEfforts);
}
