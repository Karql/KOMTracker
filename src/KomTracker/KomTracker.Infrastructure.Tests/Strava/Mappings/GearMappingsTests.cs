using FluentAssertions;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Infrastructure.Strava.Mappings;
using Xunit;
using ApiGear = Strava.API.Client.Model.Gear;

namespace KomTracker.Infrastructure.Tests.Strava.Mappings;

public class GearMappingsTests
{
    [Theory]
    [InlineData(1, BikeType.Mountain)]
    [InlineData(2, BikeType.Cyclocross)]
    [InlineData(3, BikeType.Road)]
    [InlineData(4, BikeType.TimeTrial)]
    [InlineData(5, BikeType.Gravel)]
    [InlineData(0, BikeType.Other)]
    [InlineData(99, BikeType.Other)]
    [InlineData(null, BikeType.Other)]
    public void FrameTypeToBikeType_maps_known_and_falls_back_to_other(int? frameType, BikeType expected)
    {
        GearMappings.FrameTypeToBikeType(frameType).Should().Be(expected);
    }

    [Fact]
    public void ToStravaBikeEntity_maps_detailed_gear()
    {
        var gear = new ApiGear.GearDetailedModel
        {
            Id = "b12345",
            Name = "Canyon",
            Nickname = "Grav",
            Primary = true,
            Retired = true,
            Distance = 21207353.0,
            ConvertedDistance = 21207.0,
            BrandName = "Canyon",
            ModelName = "Grail",
            FrameType = 2,
            Description = "gravel",
            Weight = 8.6f
        };

        var entity = gear.ToStravaBikeEntity(777);

        entity.Id.Should().Be("b12345");
        entity.AthleteId.Should().Be(777);
        entity.Primary.Should().BeTrue();
        entity.Retired.Should().BeTrue();
        entity.Distance.Should().Be(21207353.0);
        entity.BrandName.Should().Be("Canyon");
        entity.ModelName.Should().Be("Grail");
        entity.FrameType.Should().Be(2);
        entity.Weight.Should().BeApproximately(8.6, 0.001);
    }

    [Fact]
    public void ToStravaBikeEntity_summary_only_leaves_detailed_fields_null()
    {
        var gear = new ApiGear.GearSummaryModel { Id = "b1", Name = "Bike", Distance = 1000.0 };

        var entity = gear.ToStravaBikeEntity(1);

        entity.Id.Should().Be("b1");
        entity.BrandName.Should().BeNull();
        entity.FrameType.Should().BeNull();
        entity.Weight.Should().BeNull();
    }
}
