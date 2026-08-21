using System.Collections.Generic;
using System.Linq;
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
        Lifecycle = e.Lifecycle,
        SaleDate = e.SaleDate,
        SalePrice = e.SalePrice
    };

    public static IEnumerable<ComponentViewModel> ToViewModels(this IEnumerable<ComponentEntity> components)
        => components.Select(ToViewModel);
}
