using System.Collections.Generic;
using System.Linq;
using KomTracker.API.Shared.ViewModels.Installation;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Component;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class ComponentViewModelMappings
{
    public static ComponentViewModel ToViewModel(this ComponentEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Brand = e.Brand,
        Model = e.Model,
        Category = e.Category,
        CategoryGroup = ComponentCategoryMetadata.Group(e.Category),
        IsMetaComponent = e.IsMetaComponent,
        WeightKg = e.WeightKg,
        Notes = e.Notes,
        Price = e.Price,
        PurchasePlace = e.PurchasePlace,
        PurchaseDate = e.PurchaseDate,
        InitialDistanceKm = e.InitialDistanceKm,
        InitialMovingHours = e.InitialMovingHours,
        InitialElevationM = e.InitialElevationM,
        WarehouseId = e.WarehouseId,
        WarehouseName = e.WarehouseName,
        InstalledOnBikeId = e.InstalledOnBikeId,
        InstalledOnBikeName = e.InstalledOnBikeName,
        InstalledPosition = e.InstalledPosition,
        InstalledBikeCount = e.InstalledBikeCount,
        ParentComponentId = e.ParentComponentId,
        ParentComponentName = e.ParentComponentName,
        CurrentPlacements = e.CurrentPlacements.Select(InstallationViewModelMappings.ToViewModel).ToArray(),
        Children = e.Children.Select(InstallationViewModelMappings.ToViewModel).ToArray(),
        Lifecycle = e.Lifecycle,
        SaleDate = e.SaleDate,
        SalePrice = e.SalePrice
    };

    public static IEnumerable<ComponentViewModel> ToViewModels(this IEnumerable<ComponentEntity> components)
        => components.Select(ToViewModel);
}
