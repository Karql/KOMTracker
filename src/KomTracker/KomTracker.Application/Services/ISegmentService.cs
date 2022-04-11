using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface ISegmentService
{
    Task<IEnumerable<(SegmentEffortEntity, SegmentEntity)>> GetActualKomsAsync(int athleteId, string token);

    /// <summary>
    /// Get last koms summary efforts
    /// null when not exists
    /// </summary>
    Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId);

    Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(int athleteId, DateTime dateFrom);

    Task<IEnumerable<KomsSummaryEntity>> GetKomsSummariesAsync(int athleteId, DateTime dateFrom);

    ComparedEffortsModel CompareEfforts(IEnumerable<SegmentEffortEntity> actualKomsEfforts, IEnumerable<SegmentEffortEntity> lastKomsEfforts, bool firstCompare = false);

    Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments);

    Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEfforts);

    Task AddNewKomsSummaryWithEffortsAsync(int athleteId, ComparedEffortsModel comparedEfforts);

    Task<IEnumerable<SegmentEntity>> GetSegmentsToRefreshAsync(int top = 100, TimeSpan? minTimeFromLastRefresh = null);

    Task UpdateSegmentsAsync(IEnumerable<SegmentEntity> segments);
}
