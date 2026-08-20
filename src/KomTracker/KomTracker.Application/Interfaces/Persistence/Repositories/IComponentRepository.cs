using KomTracker.Domain.Entities.Component;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IComponentRepository : IRepository
{
    Task<IEnumerable<ComponentEntity>> GetComponentsAsync(string userId, bool includeInactive);
    Task<ComponentEntity?> GetComponentAsync(int id);
    void AddComponent(ComponentEntity component);
    void UpdateComponent(ComponentEntity component);
    void DeleteComponent(ComponentEntity component);

    /// <summary>Clear the warehouse reference on every component pointing at the given warehouse (used when a warehouse is deleted).</summary>
    Task ClearWarehouseAsync(int warehouseId);
}
