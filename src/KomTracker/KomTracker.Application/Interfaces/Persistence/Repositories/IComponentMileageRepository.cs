using KomTracker.Domain.Entities.Component;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IComponentMileageRepository : IRepository
{
    /// <summary>The stored mileage projection for a component, or null if never computed.</summary>
    Task<ComponentMileageEntity?> GetAsync(int componentId);

    /// <summary>Stored projections for the given components (batch) — for list/detail reads.</summary>
    Task<IEnumerable<ComponentMileageEntity>> GetByComponentIdsAsync(IReadOnlyCollection<int> componentIds);

    /// <summary>Insert-or-replace the component's projection row (recompute-from-source).</summary>
    Task UpsertAsync(ComponentMileageEntity mileage);
}
