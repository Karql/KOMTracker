using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface ISegmentRepository : IRepository
{
    Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments);

    Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEffots);

    Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId);

    Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(IEnumerable<int> athleteIds, DateTime? dateFrom, int? top = null);

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

    Task<IEnumerable<SegmentEffortEntity>> GetSegmentEffortsAsync(HashSet<long> ids);

    #region KOM takeovers (battle field)

    /// <summary>New/Lost/Returned changes of a single koms_summary (with athlete sex, segment, effort).</summary>
    Task<IEnumerable<KomTakeoverChangeModel>> GetSummaryChangesForTakeoverAsync(int komsSummaryId);

    /// <summary>
    /// Opposite-side New/Lost changes of other athletes on the given segments, taken only from
    /// summaries with a smaller id and within the backward time window (by audit_cd).
    /// </summary>
    Task<IEnumerable<KomTakeoverChangeModel>> GetCounterpartChangesAsync(int komsSummaryId, IEnumerable<long> segmentIds, TimeSpan window, int excludeAthleteId);

    /// <summary>Non-reverted takeovers whose lost effort is in the given set (tracked - for revert update).</summary>
    Task<IEnumerable<KomTakeoverEntity>> GetActiveTakeoversByLostEffortAsync(IEnumerable<long> lostSegmentEffortIds);

    /// <summary>Insert takeovers, ignoring rows whose taken effort already exists (idempotent).</summary>
    Task AddTakeoversIfNotExistsAsync(IEnumerable<KomTakeoverEntity> takeovers);

    #endregion
}
