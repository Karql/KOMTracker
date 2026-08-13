#nullable enable
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Strava;
using ApiGear = Strava.API.Client.Model.Gear;

namespace KomTracker.Infrastructure.Strava.Mappings;

/// <summary>Explicit Strava gear → entity mapping (no AutoMapper for new code — D-P0-13).</summary>
public static class GearMappings
{
    /// <summary>
    /// Strava frame_type → BikeType. The full Strava set (their web gear dropdown):
    /// 1=Mountain, 2=Cross, 3=Road, 4=TT, 5=Gravel. Anything else/null → Other.
    /// </summary>
    public static BikeType FrameTypeToBikeType(int? frameType) => frameType switch
    {
        1 => BikeType.Mountain,
        2 => BikeType.Cyclocross,
        3 => BikeType.Road,
        4 => BikeType.TimeTrial,
        5 => BikeType.Gravel,
        _ => BikeType.Other
    };

    /// <summary>Map a hydrated DetailedGear (brand/model/frame_type/weight) to the raw strava.bike mirror.</summary>
    public static StravaBikeEntity ToStravaBikeEntity(this ApiGear.GearDetailedModel d, int athleteId)
    {
        var entity = ((ApiGear.GearSummaryModel)d).ToStravaBikeEntity(athleteId);
        entity.BrandName = d.BrandName;
        entity.ModelName = d.ModelName;
        entity.FrameType = d.FrameType;
        entity.Description = d.Description;
        entity.Weight = d.Weight;
        return entity;
    }

    /// <summary>Summary-only fallback (used when the DetailedGear hydration couldn't be fetched).</summary>
    public static StravaBikeEntity ToStravaBikeEntity(this ApiGear.GearSummaryModel s, int athleteId)
    {
        return new StravaBikeEntity
        {
            Id = s.Id,
            AthleteId = athleteId,
            Name = s.Name,
            Nickname = s.Nickname,
            Primary = s.Primary,
            Retired = s.Retired,
            Distance = s.Distance,
            ConvertedDistance = s.ConvertedDistance
        };
    }
}
