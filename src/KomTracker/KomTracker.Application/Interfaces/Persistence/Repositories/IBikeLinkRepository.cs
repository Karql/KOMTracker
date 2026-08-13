using KomTracker.Domain.Entities.Bike;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IBikeLinkRepository : IRepository
{
    void Add(BikeLinkEntity bikeLink);
    void Remove(BikeLinkEntity bikeLink);

    /// <summary>True when a link already exists for this external gear (unique per service+id).</summary>
    Task<bool> ExistsAsync(ExternalService externalService, string externalId);

    /// <summary>The single link for this external gear (unique per service+id), or null.</summary>
    Task<BikeLinkEntity?> GetByExternalIdAsync(ExternalService externalService, string externalId);

    /// <summary>Links for the given external ids (resolves gear id -&gt; bt.bike, for "Linked" badges).</summary>
    Task<IEnumerable<BikeLinkEntity>> GetByExternalIdsAsync(ExternalService externalService, IReadOnlyCollection<string> externalIds);

    /// <summary>Links for the given bt.bike ids (to show a bike's external links).</summary>
    Task<IEnumerable<BikeLinkEntity>> GetByBikeIdsAsync(IReadOnlyCollection<int> bikeIds);
}
