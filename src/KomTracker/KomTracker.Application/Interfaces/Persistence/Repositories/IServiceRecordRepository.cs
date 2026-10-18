using KomTracker.Domain.Entities.ServiceRecord;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IServiceRecordRepository : IRepository
{
    /// <summary>The user's services with their targets, newest first.</summary>
    Task<IEnumerable<ServiceRecordEntity>> GetServiceRecordsAsync(string userId);

    /// <summary>One service with its targets (tracked — for updates/deletes).</summary>
    Task<ServiceRecordEntity?> GetServiceRecordAsync(int id);

    /// <summary>Services covering this bike (with all their targets), newest first.</summary>
    Task<IEnumerable<ServiceRecordEntity>> GetByBikeAsync(int bikeId);

    /// <summary>Services covering this component (with all their targets), newest first.</summary>
    Task<IEnumerable<ServiceRecordEntity>> GetByComponentAsync(int componentId);

    void AddServiceRecord(ServiceRecordEntity service);
    void DeleteServiceRecord(ServiceRecordEntity service);

    /// <summary>True when any service covers this bike (delete guard, D-SRV-6).</summary>
    Task<bool> AnyByBikeAsync(int bikeId);

    /// <summary>True when any service covers this component (delete guard, D-SRV-6).</summary>
    Task<bool> AnyByComponentAsync(int componentId);

    /// <summary>
    /// Targets to recompute (tracked, with <see cref="ServiceTargetEntity.Service"/> loaded for its date): every target of
    /// the given services plus every target pointing at the given components / bikes.
    /// </summary>
    Task<IReadOnlyList<ServiceTargetEntity>> GetTargetsForRecalculationAsync(
        IReadOnlyCollection<int> serviceIds, IReadOnlyCollection<int> componentIds, IReadOnlyCollection<int> bikeIds);

    /// <summary>Every target of every service (tracked, with its service) — admin rebuild.</summary>
    Task<IReadOnlyList<ServiceTargetEntity>> GetAllTargetsAsync();

    /// <summary>DB-side DISTINCT of the non-empty Place values for the user's services (autocomplete hints).</summary>
    Task<IReadOnlyList<string>> GetDistinctPlacesAsync(string userId);
}
