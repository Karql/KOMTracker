using System.Collections.Generic;
using FluentAssertions;
using KomTracker.Application.Models.Strava;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using Xunit;

namespace KomTracker.Application.Tests.Services;

public class BikeTotalsCalculatorTests
{
    private static GearTotalsModel Gear(string id, double m, long s, double elev, int count) =>
        new() { GearId = id, DistanceMeters = m, MovingTimeSeconds = s, ElevationMeters = elev, ActivityCount = count };

    [Fact]
    public void Initial_only_when_no_links()
    {
        var bike = new BikeEntity { InitialDistanceKm = 100m, InitialMovingHours = 5m, InitialElevationM = 200m };

        var t = BikeTotalsCalculator.Compute(bike, new Dictionary<string, GearTotalsModel>());

        t.DistanceKm.Should().Be(100m);
        t.MovingHours.Should().Be(5m);
        t.ElevationM.Should().Be(200m);
        t.ActivityCount.Should().Be(0);
    }

    [Fact]
    public void Adds_activity_totals_to_initial()
    {
        var bike = new BikeEntity
        {
            InitialDistanceKm = 100m,
            InitialMovingHours = 5m,
            InitialElevationM = 200m,
            Links = new List<BikeLinkEntity> { new() { ExternalService = ExternalService.Strava, ExternalId = "b1" } }
        };
        // 50 km, 2 h, 300 m over 3 rides.
        var totals = new Dictionary<string, GearTotalsModel> { ["b1"] = Gear("b1", 50_000, 7_200, 300, 3) };

        var t = BikeTotalsCalculator.Compute(bike, totals);

        t.DistanceKm.Should().Be(150m);
        t.MovingHours.Should().Be(7m);
        t.ElevationM.Should().Be(500m);
        t.ActivityCount.Should().Be(3);
    }

    [Fact]
    public void Sums_across_multiple_strava_links()
    {
        var bike = new BikeEntity
        {
            InitialDistanceKm = 0m,
            Links = new List<BikeLinkEntity>
            {
                new() { ExternalService = ExternalService.Strava, ExternalId = "b1" },
                new() { ExternalService = ExternalService.Strava, ExternalId = "b2" }
            }
        };
        var totals = new Dictionary<string, GearTotalsModel>
        {
            ["b1"] = Gear("b1", 10_000, 0, 0, 1),
            ["b2"] = Gear("b2", 30_000, 0, 0, 2)
        };

        var t = BikeTotalsCalculator.Compute(bike, totals);

        t.DistanceKm.Should().Be(40m);
        t.ActivityCount.Should().Be(3);
    }

    [Fact]
    public void Null_initial_treated_as_zero()
    {
        var bike = new BikeEntity
        {
            InitialDistanceKm = 0m,
            InitialMovingHours = null,
            InitialElevationM = null,
            Links = new List<BikeLinkEntity> { new() { ExternalService = ExternalService.Strava, ExternalId = "b1" } }
        };
        var totals = new Dictionary<string, GearTotalsModel> { ["b1"] = Gear("b1", 0, 3_600, 100, 1) };

        var t = BikeTotalsCalculator.Compute(bike, totals);

        t.MovingHours.Should().Be(1m);
        t.ElevationM.Should().Be(100m);
    }

    [Fact]
    public void Ignores_gear_not_in_totals_and_non_strava_links()
    {
        var bike = new BikeEntity
        {
            InitialDistanceKm = 10m,
            Links = new List<BikeLinkEntity>
            {
                new() { ExternalService = ExternalService.Strava, ExternalId = "b-unknown" },
                new() { ExternalService = ExternalService.Other, ExternalId = "b1" }
            }
        };
        var totals = new Dictionary<string, GearTotalsModel> { ["b1"] = Gear("b1", 99_000, 0, 0, 9) };

        var t = BikeTotalsCalculator.Compute(bike, totals);

        t.DistanceKm.Should().Be(10m); // unknown gear + non-Strava link contribute nothing
        t.ActivityCount.Should().Be(0);
    }
}
