using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface ISegmentService
{
    Task<IEnumerable<(SegmentEffortEntity, SegmentEntity)>> GetActualKomsAsync(int athleteId, string token);

    Task<IEnumerable<SegmentEffortEntity>> GetLastKomsSummaryEffortsAsync(int athleteId);

    ComparedEffortsModel CompareEfforts(IEnumerable<SegmentEffortEntity> actualEfforts, IEnumerable<SegmentEffortEntity> lastEfforts);

    Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments);

    Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEfforts);

    Task AddNewKomsSummaryWithEffortsAsync(int athleteId, ComparedEffortsModel comparedEfforts);
}
