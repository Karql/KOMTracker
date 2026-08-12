using FluentAssertions;
using KomTracker.Infrastructure.Strava.Mappings;
using System;
using Xunit;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Tests.Strava.Mappings;

public class ActivityMappingsTests
{
    [Fact]
    public void ToEntity_maps_core_fields_and_splits_latlng()
    {
        var model = new ApiModel.Activity.ActivitySummaryModel
        {
            Id = 123,
            GearId = "b805524",
            Name = "Afternoon Ride",
            SportType = "Ride",
            Distance = 3830.6f,
            UtcOffset = 7200f,
            StartDate = new DateTime(2026, 8, 6, 13, 44, 38, DateTimeKind.Utc),
            StartLatlng = new[] { 50.08f, 19.99f },
            EndLatlng = new[] { 50.06f, 20.02f },
            Map = new ApiModel.Base.PolylineMapModel { SummaryPolyline = "abc123" }
        };

        var entity = model.ToEntity(2394302);

        entity.Id.Should().Be(123);
        entity.AthleteId.Should().Be(2394302);
        entity.GearId.Should().Be("b805524");
        entity.SportType.Should().Be("Ride");
        entity.Distance.Should().BeApproximately(3830.6, 0.01);
        entity.UtcOffset.Should().Be(7200);
        entity.StartDate.Should().Be(new DateTime(2026, 8, 6, 13, 44, 38, DateTimeKind.Utc));
        entity.StartLat.Should().BeApproximately(50.08, 0.001);
        entity.StartLng.Should().BeApproximately(19.99, 0.001);
        entity.EndLat.Should().BeApproximately(50.06, 0.001);
        entity.EndLng.Should().BeApproximately(20.02, 0.001);
        entity.SummaryPolyline.Should().Be("abc123");
    }

    [Fact]
    public void ToEntity_handles_missing_latlng_and_map()
    {
        var model = new ApiModel.Activity.ActivitySummaryModel
        {
            Id = 1,
            Name = "Manual",
            SportType = "Ride",
            StartLatlng = null,
            EndLatlng = null,
            Map = null
        };

        var entity = model.ToEntity(1);

        entity.StartLat.Should().BeNull();
        entity.StartLng.Should().BeNull();
        entity.EndLat.Should().BeNull();
        entity.EndLng.Should().BeNull();
        entity.SummaryPolyline.Should().BeNull();
    }
}
