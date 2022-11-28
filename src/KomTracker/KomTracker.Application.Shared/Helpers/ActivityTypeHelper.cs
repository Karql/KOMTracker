using KomTracker.Application.Shared.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Shared.Helpers;

public static class ActivityTypeConsts
{
    public const string Ride = "Ride";
    public const string Run = "Run";
    public const string Swim = "Swim";
    public const string Hike = "Hike";
    public const string Walk = "Walk";
    public const string AlpineSki = "AlpineSki";
    public const string BackcountrySki = "BackcountrySki";
    public const string EBikeRide = "EBikeRide";
    public const string InlineSkate = "InlineSkate";
    public const string NordicSki = "NordicSki";
    public const string Snowboard = "Snowboard";
    public const string VirtualRide = "VirtualRide";
    public const string WaterSport = "WaterSport";
}
public static class ActivityTypeHelper
{
    private record class ActivityTypeDescriptionModel(string ActivityType, string Name, int Order);

    private static readonly Dictionary<string, ActivityTypeDescriptionModel> _activityTypes = new Dictionary<string, ActivityTypeDescriptionModel>
    {
        { ActivityTypeConsts.Ride, new (ActivityTypeConsts.Ride, "Ride", 1) },
        { ActivityTypeConsts.Run, new (ActivityTypeConsts.Run, "Run", 2) },
        { ActivityTypeConsts.Swim, new (ActivityTypeConsts.Swim, "Swim", 3) },
        { ActivityTypeConsts.Hike, new (ActivityTypeConsts.Hike, "Hike", 4) },
        { ActivityTypeConsts.Walk, new (ActivityTypeConsts.Walk, "Walk", 5) },
        { ActivityTypeConsts.AlpineSki, new (ActivityTypeConsts.AlpineSki, "Alpine Ski", 6) },
        { ActivityTypeConsts.BackcountrySki, new (ActivityTypeConsts.BackcountrySki, "Backcountry Ski", 7) },
        { ActivityTypeConsts.EBikeRide, new (ActivityTypeConsts.EBikeRide, "E-Bike Ride", 8) },
        { ActivityTypeConsts.InlineSkate, new (ActivityTypeConsts.InlineSkate, "Inline Skate", 9) },
        { ActivityTypeConsts.NordicSki, new (ActivityTypeConsts.NordicSki, "Nordic Ski", 10) },
        { ActivityTypeConsts.Snowboard, new (ActivityTypeConsts.Snowboard, "Snowboard", 11) },
        { ActivityTypeConsts.VirtualRide, new (ActivityTypeConsts.VirtualRide, "Virtual Ride", 12) },
        { ActivityTypeConsts.WaterSport, new (ActivityTypeConsts.WaterSport, "Water Sport", 13) },
    };

    public static string GetActivityTypeName(string activityType)
    {
        if (_activityTypes.TryGetValue(activityType, out var type))
        {
            return type.Name;
        }

        return activityType;
    }

    public static int GetActivityTypeOrder(string activityType)
    {
        if (_activityTypes.TryGetValue(activityType, out var type))
        {
            return type.Order;
        }

        return int.MaxValue;
    }

    public static IEnumerable<(string ActivityType, string Name)> GetActivityTypes()
    {
        return _activityTypes
            .OrderBy(x => x.Value.Order)
            .Select(x => (x.Value.ActivityType, x.Value.Name))
            .ToArray();
    }
}
