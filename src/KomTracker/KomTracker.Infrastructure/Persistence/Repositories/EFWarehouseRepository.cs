#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFWarehouseRepository : EFBaseRepository, IWarehouseRepository
{
    public async Task<IEnumerable<WarehouseEntity>> GetWarehousesAsync(string userId)
    {
        return await _context.Warehouse
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<WarehouseEntity?> GetWarehouseAsync(int id)
    {
        return await _context.Warehouse.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void AddWarehouse(WarehouseEntity warehouse)
    {
        _context.Warehouse.Add(warehouse);
    }

    public void UpdateWarehouse(WarehouseEntity warehouse)
    {
        _context.Warehouse.Update(warehouse);
    }

    public void DeleteWarehouse(WarehouseEntity warehouse)
    {
        _context.Warehouse.Remove(warehouse);
    }
}
