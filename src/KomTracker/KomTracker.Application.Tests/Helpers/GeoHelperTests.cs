using FluentAssertions;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Segment;
using Xunit;

namespace KomTracker.Application.Tests.Helpers;

public class GeoHelperTests
{
    [Theory]
    [InlineData(0, 0, 1, 0, 0)]      // due north
    [InlineData(0, 0, 0, 1, 90)]     // due east
    [InlineData(1, 0, 0, 0, 180)]    // due south
    [InlineData(0, 1, 0, 0, 270)]    // due west
    public void GetBearing_returns_expected_cardinal_bearing(double lat1, double lon1, double lat2, double lon2, double expected)
    {
        GeoHelper.GetBearing(lat1, lon1, lat2, lon2).Should().BeApproximately(expected, 0.01);
    }

    [Theory]
    [InlineData(0, CompassDirection.N)]
    [InlineData(45, CompassDirection.NE)]
    [InlineData(90, CompassDirection.E)]
    [InlineData(135, CompassDirection.SE)]
    [InlineData(180, CompassDirection.S)]
    [InlineData(225, CompassDirection.SW)]
    [InlineData(270, CompassDirection.W)]
    [InlineData(315, CompassDirection.NW)]
    [InlineData(10, CompassDirection.N)]
    [InlineData(350, CompassDirection.N)]
    [InlineData(337.5, CompassDirection.N)]
    public void GetCompassDirection_buckets_by_45_degrees(double bearing, CompassDirection expected)
    {
        GeoHelper.GetCompassDirection(bearing).Should().Be(expected);
    }
}
