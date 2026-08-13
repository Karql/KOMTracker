#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFBikeLinkRepository : EFBaseRepository, IBikeLinkRepository
{
    public void Add(BikeLinkEntity bikeLink)
    {
        _context.BikeLink.Add(bikeLink);
    }

    public void Remove(BikeLinkEntity bikeLink)
    {
        _context.BikeLink.Remove(bikeLink);
    }

    public Task<bool> ExistsAsync(ExternalService externalService, string externalId)
    {
        return _context.BikeLink.AsNoTracking()
            .AnyAsync(x => x.ExternalService == externalService && x.ExternalId == externalId);
    }

    public Task<BikeLinkEntity?> GetByExternalIdAsync(ExternalService externalService, string externalId)
    {
        return _context.BikeLink
            .FirstOrDefaultAsync(x => x.ExternalService == externalService && x.ExternalId == externalId);
    }

    public async Task<IEnumerable<BikeLinkEntity>> GetByExternalIdsAsync(ExternalService externalService, IReadOnlyCollection<string> externalIds)
    {
        return await _context.BikeLink.AsNoTracking()
            .Where(x => x.ExternalService == externalService && externalIds.Contains(x.ExternalId))
            .ToListAsync();
    }

    public async Task<IEnumerable<BikeLinkEntity>> GetByBikeIdsAsync(IReadOnlyCollection<int> bikeIds)
    {
        return await _context.BikeLink.AsNoTracking()
            .Where(x => bikeIds.Contains(x.BikeId))
            .ToListAsync();
    }
}
