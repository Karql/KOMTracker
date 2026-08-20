namespace KomTracker.API.Shared.ViewModels.Warehouse;

/// <summary>A warehouse (place holding non-installed components).</summary>
public class WarehouseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}
