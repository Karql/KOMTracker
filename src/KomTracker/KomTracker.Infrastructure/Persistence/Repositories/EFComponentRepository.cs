#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFComponentRepository : EFBaseRepository, IComponentRepository
{
    public async Task<IEnumerable<ComponentEntity>> GetComponentsAsync(string userId, bool includeInactive)
    {
        var query = _context.Component
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!includeInactive)
        {
            query = query.Where(x => x.Lifecycle == ComponentLifecycle.Active);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<ComponentEntity?> GetComponentAsync(int id)
    {
        return await _context.Component.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ComponentEntity>> GetByIdsAsync(IReadOnlyCollection<int> componentIds)
    {
        if (componentIds is null || componentIds.Count == 0)
        {
            return Enumerable.Empty<ComponentEntity>();
        }

        return await _context.Component.AsNoTracking()
            .Where(x => componentIds.Contains(x.Id))
            .ToListAsync();
    }

    public void AddComponent(ComponentEntity component)
    {
        _context.Component.Add(component);
    }

    public void UpdateComponent(ComponentEntity component)
    {
        _context.Component.Update(component);
    }

    public void DeleteComponent(ComponentEntity component)
    {
        _context.Component.Remove(component);
    }

    public async Task ClearWarehouseAsync(int warehouseId)
    {
        await _context.Component
            .Where(x => x.WarehouseId == warehouseId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.WarehouseId, (int?)null));
    }
}
