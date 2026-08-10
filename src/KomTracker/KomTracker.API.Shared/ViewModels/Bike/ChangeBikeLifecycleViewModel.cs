using KomTracker.Domain.Entities.Bike;

namespace KomTracker.API.Shared.ViewModels.Bike;

/// <summary>Request body for changing a bike's lifecycle. SaleDate/SalePrice required when Sold.</summary>
public class ChangeBikeLifecycleViewModel
{
    public BikeLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
}
