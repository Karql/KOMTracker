using KomTracker.Domain.Entities.Component;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IInstallationRepository : IRepository
{
    /// <summary>All installations on a bike, newest first (current before historical).</summary>
    Task<IEnumerable<InstallationEntity>> GetByBikeAsync(int bikeId);

    /// <summary>All installations of a component, newest first (current before historical).</summary>
    Task<IEnumerable<InstallationEntity>> GetByComponentAsync(int componentId);

    /// <summary>The component's active Tracked installation (DateTo == null), or null.</summary>
    Task<InstallationEntity?> GetActiveTrackedByComponentAsync(int componentId);

    /// <summary>Active Tracked installations for the given components (batch), for list resolution.</summary>
    Task<IEnumerable<InstallationEntity>> GetActiveTrackedByComponentsAsync(IReadOnlyCollection<int> componentIds);

    Task<InstallationEntity?> GetAsync(int id);

    /// <summary>Whether the component has any installation record (for the delete guard — D-18).</summary>
    Task<bool> AnyByComponentAsync(int componentId);

    void Add(InstallationEntity installation);
    void Update(InstallationEntity installation);
    void Delete(InstallationEntity installation);

    /// <summary>Hard-delete all installations on a bike (used when the bike itself is deleted).</summary>
    Task DeleteByBikeAsync(int bikeId);
}
