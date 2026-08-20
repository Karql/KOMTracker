using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.Warehouse;

namespace KomTracker.API.Shared.ViewModels.Warehouse;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class WarehouseViewModelMappings
{
    public static WarehouseViewModel ToViewModel(this WarehouseEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name
    };

    public static IEnumerable<WarehouseViewModel> ToViewModels(this IEnumerable<WarehouseEntity> warehouses)
        => warehouses.Select(ToViewModel);
}
