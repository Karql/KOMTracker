using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class InstallationViewModelMappings
{
    public static InstallationViewModel ToViewModel(this InstallationEntity e) => new()
    {
        Id = e.Id,
        ComponentId = e.ComponentId,
        ComponentName = e.ComponentName,
        ComponentCategory = e.ComponentCategory,
        BikeId = e.BikeId,
        BikeName = e.BikeName,
        Type = e.Type,
        DateFrom = e.DateFrom,
        DateTo = e.DateTo,
        Position = e.Position,
        ManualDistanceKm = e.ManualDistanceKm,
        ManualMovingHours = e.ManualMovingHours,
        ManualElevationM = e.ManualElevationM,
        IsCurrent = e.IsCurrent
    };

    public static IEnumerable<InstallationViewModel> ToViewModels(this IEnumerable<InstallationEntity> installations)
        => installations.Select(ToViewModel);
}
