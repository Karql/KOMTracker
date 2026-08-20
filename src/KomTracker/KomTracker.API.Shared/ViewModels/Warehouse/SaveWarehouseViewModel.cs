namespace KomTracker.API.Shared.ViewModels.Warehouse;

/// <summary>Request body for creating/updating a warehouse. Field names mirror <c>SaveWarehouseCommand</c>.</summary>
public class SaveWarehouseViewModel
{
    public string Name { get; set; } = default!;
}
