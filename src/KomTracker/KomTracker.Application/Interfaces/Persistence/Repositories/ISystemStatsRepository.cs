using KomTracker.Domain.Entities.SystemStats;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface ISystemStatsRepository : IRepository
{
    /// <summary>The whole admin stats snapshot, as of <paramref name="now"/> (UTC).</summary>
    Task<SystemStatsReport> GetReportAsync(DateTime now);
}
