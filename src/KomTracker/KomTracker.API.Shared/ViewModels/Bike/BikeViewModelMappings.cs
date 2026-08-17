using KomTracker.Domain.Entities.Bike;

namespace KomTracker.API.Shared.ViewModels.Bike;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class BikeViewModelMappings
{
    public static BikeViewModel ToViewModel(this BikeEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Brand = e.Brand,
        Model = e.Model,
        Type = e.Type,
        WeightKg = e.WeightKg,
        Notes = e.Notes,
        Price = e.Price,
        PurchasePlace = e.PurchasePlace,
        PurchaseDate = e.PurchaseDate,
        InitialDistanceKm = e.InitialDistanceKm,
        InitialMovingHours = e.InitialMovingHours,
        InitialElevationM = e.InitialElevationM,
        Lifecycle = e.Lifecycle,
        SaleDate = e.SaleDate,
        SalePrice = e.SalePrice,
        StravaGearId = e.Links.FirstOrDefault(l => l.ExternalService == ExternalService.Strava)?.ExternalId,
        TotalDistanceKm = e.TotalDistanceKm,
        TotalMovingHours = e.TotalMovingHours,
        TotalElevationM = e.TotalElevationM,
        AttributedActivityCount = e.AttributedActivityCount
    };

    public static IEnumerable<BikeViewModel> ToViewModels(this IEnumerable<BikeEntity> bikes)
        => bikes.Select(ToViewModel);
}
