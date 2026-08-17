using System.Collections.Generic;
using KomTracker.Application.Models.Strava;
using KomTracker.Domain.Entities.Bike;

namespace KomTracker.Application.Services;

/// <summary>
/// Pure computation of a bike's mileage totals: initial seed + Σ of the activities attributed to it
/// (via its Strava links). Recompute-from-source (D-14) — no stored counters.
/// </summary>
public static class BikeTotalsCalculator
{
    public record BikeTotals(decimal DistanceKm, decimal MovingHours, decimal ElevationM, int ActivityCount);

    public static BikeTotals Compute(BikeEntity bike, IReadOnlyDictionary<string, GearTotalsModel> totalsByGearId)
    {
        double distanceMeters = 0;
        long movingSeconds = 0;
        double elevationMeters = 0;
        var activityCount = 0;

        foreach (var link in bike.Links)
        {
            if (link.ExternalService != ExternalService.Strava)
            {
                continue;
            }

            if (totalsByGearId.TryGetValue(link.ExternalId, out var gear))
            {
                distanceMeters += gear.DistanceMeters;
                movingSeconds += gear.MovingTimeSeconds;
                elevationMeters += gear.ElevationMeters;
                activityCount += gear.ActivityCount;
            }
        }

        return new BikeTotals(
            DistanceKm: bike.InitialDistanceKm + (decimal)(distanceMeters / 1000.0),
            MovingHours: (bike.InitialMovingHours ?? 0m) + (decimal)(movingSeconds / 3600.0),
            ElevationM: (bike.InitialElevationM ?? 0m) + (decimal)elevationMeters,
            ActivityCount: activityCount);
    }

    /// <summary>Compute and assign the totals onto the bike's read-model fields.</summary>
    public static void Apply(BikeEntity bike, IReadOnlyDictionary<string, GearTotalsModel> totalsByGearId)
    {
        var totals = Compute(bike, totalsByGearId);
        bike.TotalDistanceKm = totals.DistanceKm;
        bike.TotalMovingHours = totals.MovingHours;
        bike.TotalElevationM = totals.ElevationM;
        bike.AttributedActivityCount = totals.ActivityCount;
    }
}
