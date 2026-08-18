using KomTracker.API.Shared.ViewModels;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.Application.Models;
using KomTracker.Application.Models.Strava;
using KomTracker.Domain.Entities.Strava;
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
        StravaBikeCount = m.StravaBikeCount,
        ActivityCount = m.ActivityCount,
        Scopes = m.Scopes
    };

    public static ActivityViewModel ToViewModel(this ActivityListItemModel a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        SportType = a.SportType,
        DistanceKm = a.DistanceMeters / 1000.0,
        MovingTimeSeconds = a.MovingTimeSeconds,
        AverageSpeedKmh = a.AverageSpeedMps * 3.6,
        ElevationM = a.ElevationMeters,
        StartDateLocal = a.StartDateUtc.AddSeconds(a.UtcOffset),
        GearId = a.GearId,
        LinkedBikeId = a.LinkedBikeId,
        LinkedBikeName = a.LinkedBikeName,
        StravaBikeName = a.StravaBikeName
    };

    public static PagedResultViewModel<ActivityViewModel> ToViewModel(this PagedResultModel<ActivityListItemModel> page) => new()
    {
        Items = page.Items.Select(ToViewModel).ToArray(),
        TotalCount = page.TotalCount
    };

    public static ActivitySyncHistoryViewModel ToViewModel(this ActivitySyncHistoryEntity h) => new()
    {
        RunAt = h.RunAt,
        Duration = h.Duration,
        SyncFrom = h.SyncFrom,
        Status = h.Status,
        UpsertedCount = h.UpsertedCount,
        DeletedCount = h.DeletedCount,
        ActivitiesCount = h.ActivitiesCount
    };

    public static IEnumerable<ActivitySyncHistoryViewModel> ToViewModels(this IEnumerable<ActivitySyncHistoryEntity> history)
        => history.Select(ToViewModel);
}
