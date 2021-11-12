using KOMTracker.API.Models.Segment;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KOMTracker.API.DAL.Repositories;

public interface ISegmentRepository : IRepository
{
    Task AddSegmentsIfNotExists(IEnumerable<SegmentModel> segments);

    Task<IEnumerable<SegmentEffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId);

    Task AddKomsSummaryWithEffortsAsync(KomsSummaryModel komsSummary);
}
