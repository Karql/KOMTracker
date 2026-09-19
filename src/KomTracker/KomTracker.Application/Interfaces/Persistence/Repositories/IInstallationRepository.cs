using KomTracker.Domain.Entities.Component;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IInstallationRepository : IRepository
{
    /// <summary>All installations on a bike, newest first (current before historical).</summary>
    Task<IEnumerable<InstallationEntity>> GetByBikeAsync(int bikeId);

    /// <summary>All installations of a component, newest first (current before historical).</summary>
    Task<IEnumerable<InstallationEntity>> GetByComponentAsync(int componentId);

    /// <summary>All installations of the given components (batch) — for mileage recompute.</summary>
    Task<IEnumerable<InstallationEntity>> GetByComponentsAsync(IReadOnlyCollection<int> componentIds);

    /// <summary>Distinct component ids installed (ever) on the given bikes — for post-sync recompute targeting.</summary>
    Task<IEnumerable<int>> GetComponentIdsByBikesAsync(IReadOnlyCollection<int> bikeIds);

    /// <summary>The component's active Tracked installation (DateTo == null), or null. (Legacy single-placement helper.)</summary>
    Task<InstallationEntity?> GetActiveTrackedByComponentAsync(int componentId);

    /// <summary>ALL active Tracked installations of a component (DateTo == null) — multi-bike (2b-ii). Used by the D-7 invariant.</summary>
    Task<IEnumerable<InstallationEntity>> GetActiveTrackedInstallationsByComponentAsync(int componentId);

    /// <summary>Active Tracked installations for the given components (batch), for list resolution.</summary>
    Task<IEnumerable<InstallationEntity>> GetActiveTrackedByComponentsAsync(IReadOnlyCollection<int> componentIds);

    /// <summary>All installations whose parent is this component (component-in-component history), newest first.</summary>
    Task<IEnumerable<InstallationEntity>> GetByParentComponentAsync(int parentComponentId);

    /// <summary>Active Tracked children (installed INTO the given parent components) — batch, for list/detail resolution.</summary>
    Task<IEnumerable<InstallationEntity>> GetActiveChildrenByParentComponentsAsync(IReadOnlyCollection<int> parentComponentIds);

    /// <summary>Distinct component ids EVER installed into the given parent components (current + historical) — for mileage recompute expansion.</summary>
    Task<IEnumerable<int>> GetChildComponentIdsByParentsAsync(IReadOnlyCollection<int> parentComponentIds);

    Task<InstallationEntity?> GetAsync(int id);

    /// <summary>Whether the component has any installation record as the installed component (delete guard — D-18).</summary>
    Task<bool> AnyByComponentAsync(int componentId);

    /// <summary>Whether the component is the parent of any installation (delete guard — D-18, meta-component).</summary>
    Task<bool> AnyByParentComponentAsync(int parentComponentId);

    void Add(InstallationEntity installation);
    void Update(InstallationEntity installation);
    void Delete(InstallationEntity installation);

    /// <summary>Hard-delete all installations on a bike (used when the bike itself is deleted).</summary>
    Task DeleteByBikeAsync(int bikeId);
}
