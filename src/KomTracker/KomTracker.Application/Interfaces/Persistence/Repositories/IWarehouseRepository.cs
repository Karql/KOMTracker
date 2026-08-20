using KomTracker.Domain.Entities.Warehouse;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IWarehouseRepository : IRepository
{
    Task<IEnumerable<WarehouseEntity>> GetWarehousesAsync(string userId);
    Task<WarehouseEntity?> GetWarehouseAsync(int id);
    void AddWarehouse(WarehouseEntity warehouse);
    void UpdateWarehouse(WarehouseEntity warehouse);
    void DeleteWarehouse(WarehouseEntity warehouse);
}
