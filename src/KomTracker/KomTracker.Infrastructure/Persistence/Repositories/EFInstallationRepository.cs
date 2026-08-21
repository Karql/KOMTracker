#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFInstallationRepository : EFBaseRepository, IInstallationRepository
{
    // Current (DateTo null) first, then most-recent by DateFrom/audit.
    private static IOrderedQueryable<InstallationEntity> Ordered(IQueryable<InstallationEntity> q)
        => q.OrderBy(x => x.DateTo == null ? 0 : 1)
            .ThenByDescending(x => x.DateFrom)
            .ThenByDescending(x => x.AuditCD);

    public async Task<IEnumerable<InstallationEntity>> GetByBikeAsync(int bikeId)
        => await Ordered(_context.Installation.AsNoTracking().Where(x => x.BikeId == bikeId)).ToListAsync();

    public async Task<IEnumerable<InstallationEntity>> GetByComponentAsync(int componentId)
        => await Ordered(_context.Installation.AsNoTracking().Where(x => x.ComponentId == componentId)).ToListAsync();

    public async Task<InstallationEntity?> GetActiveTrackedByComponentAsync(int componentId)
        => await _context.Installation
            .Where(x => x.ComponentId == componentId
                && x.Type == ComponentInstallationType.Tracked && x.DateTo == null)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<InstallationEntity>> GetActiveTrackedByComponentsAsync(IReadOnlyCollection<int> componentIds)
    {
        if (componentIds is null || componentIds.Count == 0)
        {
            return Enumerable.Empty<InstallationEntity>();
        }

        return await _context.Installation.AsNoTracking()
            .Where(x => componentIds.Contains(x.ComponentId)
                && x.Type == ComponentInstallationType.Tracked && x.DateTo == null)
            .ToListAsync();
    }

    public async Task<InstallationEntity?> GetAsync(int id)
        => await _context.Installation.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> AnyByComponentAsync(int componentId)
        => await _context.Installation.AsNoTracking().AnyAsync(x => x.ComponentId == componentId);

    public void Add(InstallationEntity installation) => _context.Installation.Add(installation);

    public void Update(InstallationEntity installation) => _context.Installation.Update(installation);

    public void Delete(InstallationEntity installation) => _context.Installation.Remove(installation);

    public async Task DeleteByBikeAsync(int bikeId)
        => await _context.Installation.Where(x => x.BikeId == bikeId).ExecuteDeleteAsync();
}
