using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.Application.Models.Strava;
using KomTracker.Infrastructure.Strava.Mappings;

namespace KomTracker.API.Extensions;

/// <summary>Explicit Strava-bike model → view-model mapping (frame_type → BikeType via GearMappings, D-1c-8).</summary>
public static class StravaBikeMappings
{
    public static StravaBikeViewModel ToViewModel(this StravaBikeModel m)
    {
        var b = m.Bike;
        return new StravaBikeViewModel
        {
            Id = b.Id,
            Name = b.Name,
            Nickname = b.Nickname,
            Primary = b.Primary,
            Retired = b.Retired,
            DistanceKm = b.Distance / 1000.0,
            BrandName = b.BrandName,
            ModelName = b.ModelName,
            SuggestedType = GearMappings.FrameTypeToBikeType(b.FrameType),
            WeightKg = b.Weight.HasValue ? (decimal)b.Weight.Value : null,
            LinkedBikeId = m.LinkedBikeId,
            LinkedBikeName = m.LinkedBikeName
        };
    }

    public static IEnumerable<StravaBikeViewModel> ToViewModels(this IEnumerable<StravaBikeModel> bikes)
        => bikes.Select(ToViewModel);

    public static StravaSyncStatusViewModel ToViewModel(this StravaSyncStatusModel m) => new()
    {
        BikesEnabled = m.BikesEnabled,
        ActivitiesEnabled = m.ActivitiesEnabled,
        HasActivityReadAll = m.HasActivityReadAll,
        StravaBikeCount = m.StravaBikeCount
    };
}
