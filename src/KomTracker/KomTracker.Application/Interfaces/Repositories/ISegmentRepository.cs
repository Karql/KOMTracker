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

    Task AddKomsSummaryAsync(KomsSummaryEntity komsSummary);

    Task AddKomsSummariesSegmentEffortsAsync(IEnumerable<KomsSummarySegmentEffortEntity> komsSummariesSegmentEfforts);
}
