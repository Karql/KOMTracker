using KomTracker.Domain.Entities.Component;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IComponentRepository : IRepository
{
    Task<IEnumerable<ComponentEntity>> GetComponentsAsync(string userId, bool includeInactive);
    Task<ComponentEntity?> GetComponentAsync(int id);

    /// <summary>Components by id (batch) — for mileage recompute (not user-scoped).</summary>
    Task<IEnumerable<ComponentEntity>> GetByIdsAsync(IReadOnlyCollection<int> componentIds);
    void AddComponent(ComponentEntity component);
    void UpdateComponent(ComponentEntity component);
    void DeleteComponent(ComponentEntity component);

    /// <summary>Clear the warehouse reference on every component pointing at the given warehouse (used when a warehouse is deleted).</summary>
    Task ClearWarehouseAsync(int warehouseId);

    /// <summary>DB-side DISTINCT of the non-empty Brand / Model / Purchase place values for the user's components (autocomplete hints).</summary>
    Task<(IReadOnlyList<string> Brands, IReadOnlyList<string> Models, IReadOnlyList<string> PurchasePlaces)> GetDistinctPurchaseFieldsAsync(string userId);
}
