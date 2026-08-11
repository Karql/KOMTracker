using Strava.API.Client.Model.Activity;
using Strava.API.Client.Tests.Extensions.Model.Athlete;
using Strava.API.Client.Tests.Extensions.Model.Base;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Activity;

public static class ActivitySummaryModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response.
    /// </summary>
    /// <remarks>Manually created string for testing deserialization.</remarks>
    public static string ToJson(this ActivitySummaryModel model)
    {
        return @$"{{
                ""resource_state"": {(int)model.ResourceState},
                ""id"": {model.Id},
                ""athlete"": {model.Athlete.ToJson()},
                ""name"": ""{model.Name}"",
                ""external_id"": ""{model.ExternalId}"",
                ""upload_id"": {model.UploadId},
                ""gear_id"": ""{model.GearId}"",
                ""distance"": {model.Distance},
                ""moving_time"": {model.MovingTime},
                ""elapsed_time"": {model.ElapsedTime},
                ""total_elevation_gain"": {model.TotalElevationGain},
                ""elev_high"": {model.ElevHigh},
                ""elev_low"": {model.ElevLow},
                ""type"": ""{model.Type}"",
                ""sport_type"": ""{model.SportType}"",
                ""workout_type"": {model.WorkoutType},
                ""start_date"": ""{model.StartDate.ToUtcIso()}"",
                ""start_date_local"": ""{model.StartDateLocal.ToUtcIso()}"",
                ""timezone"": ""{model.Timezone}"",
                ""utc_offset"": {model.UtcOffset},
                ""trainer"": {model.Trainer.ToLowerString()},
                ""commute"": {model.Commute.ToLowerString()},
                ""manual"": {model.Manual.ToLowerString()},
                ""private"": {model.Private.ToLowerString()},
                ""flagged"": {model.Flagged.ToLowerString()},
                ""visibility"": ""{model.Visibility}"",
                ""average_speed"": {model.AverageSpeed},
                ""max_speed"": {model.MaxSpeed},
                ""average_cadence"": {model.AverageCadence},
                ""average_temp"": {model.AverageTemp},
                ""average_watts"": {model.AverageWatts},
                ""weighted_average_watts"": {model.WeightedAverageWatts},
                ""max_watts"": {model.MaxWatts},
                ""device_watts"": {model.DeviceWatts.ToLowerString()},
                ""kilojoules"": {model.Kilojoules},
                ""has_heartrate"": {model.HasHeartrate.ToLowerString()},
                ""average_heartrate"": {model.AverageHeartrate},
                ""max_heartrate"": {model.MaxHeartrate},
                ""suffer_score"": {model.SufferScore},
                ""achievement_count"": {model.AchievementCount},
                ""kudos_count"": {model.KudosCount},
                ""comment_count"": {model.CommentCount},
                ""athlete_count"": {model.AthleteCount},
                ""photo_count"": {model.PhotoCount},
                ""total_photo_count"": {model.TotalPhotoCount},
                ""pr_count"": {model.PrCount},
                ""map"": {model.Map.ToJson()},
                ""start_latlng"": {LatLngToJson(model.StartLatlng)},
                ""end_latlng"": {LatLngToJson(model.EndLatlng)},
                ""device_name"": ""{model.DeviceName}""
            }}";
    }

    /// <summary>
    /// JSON equivalent to API response.
    /// </summary>
    /// <remarks>Manually created string for testing deserialization.</remarks>
    public static string ToJson(this IEnumerable<ActivitySummaryModel> list)
    {
        return @$"[
                {string.Join(", ", list.Select(x => x.ToJson()))}
            ]";
    }

    private static string LatLngToJson(float[] latLng)
    {
        if (latLng is null)
        {
            return "null";
        }

        return "[" + string.Join(", ", latLng.Select(x => x.ToString(CultureInfo.InvariantCulture))) + "]";
    }
}
