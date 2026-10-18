using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.ServiceRecord;

namespace KomTracker.API.Shared.ViewModels.ServiceRecord;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class ServiceRecordViewModelMappings
{
    public static ServiceRecordViewModel ToViewModel(this ServiceRecordEntity s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Date = s.Date,
        Place = s.Place,
        Cost = s.Cost,
        Notes = s.Notes,
        Targets = s.Targets.Select(ToViewModel).ToList()
    };

    public static ServiceTargetViewModel ToViewModel(this ServiceTargetEntity t) => new()
    {
        BikeId = t.BikeId,
        ComponentId = t.ComponentId,
        Name = t.TargetName,
        Category = t.TargetCategory,
        DistanceKm = t.DistanceKm,
        MovingHours = t.MovingHours,
        ElevationM = t.ElevationM
    };

    public static IEnumerable<ServiceRecordViewModel> ToViewModels(this IEnumerable<ServiceRecordEntity> services)
        => services.Select(ToViewModel);
}
