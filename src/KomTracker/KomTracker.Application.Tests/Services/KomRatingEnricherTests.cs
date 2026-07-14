using FluentAssertions;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using Xunit;

namespace KomTracker.Application.Tests.Services;

public class KomRatingEnricherTests
{
    private static EffortModel RideEffort() => new()
    {
        Segment = new SegmentEntity { ActivityType = "Ride", Distance = 1000, AverageGrade = 5 },
        SegmentEffort = new SegmentEffortEntity { ElapsedTime = 120, AverageWatts = 300, DeviceWatts = true },
    };

    [Fact]
    public void Apply_sets_bar_and_burn_for_a_ride_with_power_and_weight()
    {
        var effort = RideEffort();

        KomRatingEnricher.Apply(effort, "M", 70);

        effort.Bar.Should().NotBeNull();
        effort.Burn.Should().NotBeNull();
    }

    [Fact]
    public void Apply_leaves_burn_null_without_weight()
    {
        var effort = RideEffort();

        KomRatingEnricher.Apply(effort, "M", 0);

        effort.Bar.Should().NotBeNull();
        effort.Burn.Should().BeNull();
    }

    [Fact]
    public void Apply_is_noop_when_segment_missing()
    {
        var effort = new EffortModel { SegmentEffort = new SegmentEffortEntity { ElapsedTime = 120 } };

        KomRatingEnricher.Apply(effort, "M", 70);

        effort.Bar.Should().BeNull();
        effort.Burn.Should().BeNull();
    }
}
