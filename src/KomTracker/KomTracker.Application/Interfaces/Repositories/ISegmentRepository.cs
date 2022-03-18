using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface ISegmentRepository : IRepository
{
    Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments);

    Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEffots);

    Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId);

    Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(int athleteId, DateTime dateFrom);

    Task<IEnumerable<KomsSummaryEntity>> GetKomsSummariesAsync(int athleteId, DateTime dateFrom);

    Task AddKomsSummaryAsync(KomsSummaryEntity komsSummary);

    Task AddKomsSummariesSegmentEffortsAsync(IEnumerable<KomsSummarySegmentEffortEntity> komsSummariesSegmentEfforts);

    /// <summary>
    /// Get segments to refresh
    /// </summary>
    /// <param name="top">Number of segment to refresh (default: 100)</param>
    /// <param name="minTimeFromLastRefresh">Minimum time from last refresh (default: 24 hours)</param>
    /// <returns></returns>
    Task<IEnumerable<SegmentEntity>> GetSegmentsToRefreshAsync(int top = 100, TimeSpan? minTimeFromLastRefresh = null);

    Task UpdateSegmentsAsync(IEnumerable<SegmentEntity> segments);
}
