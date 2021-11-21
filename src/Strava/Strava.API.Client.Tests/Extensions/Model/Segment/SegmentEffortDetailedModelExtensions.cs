using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strava.API.Client.Tests.Extensions.Model.Athlete;
using Strava.API.Client.Tests.Extensions.Model.Activity;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Segment;

public static class SegmentEffortDetailedModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this SegmentEffortDetailedModel model)
    {
        return @$"{{
                ""id"": {model.Id},
                ""resource_state"": {(int)model.ResourceState},
                ""id"": {model.Id},       
                ""activity"": {model.Activity.ToJson()},
                ""athlete"": {model.Athlete.ToJson()},
                ""segment"": {model.Segment.ToJson()},
                ""name"": ""{model.Name}"",
                ""elapsed_time"": {model.ElapsedTime},
                ""moving_time"": {model.MovingTime},
                ""start_date"": ""{model.StartDate.ToUtcIso()}"",
                ""start_date_local"": ""{model.StartDateLocal.ToUtcIso()}"",
                ""distance"": {model.Distance},
                ""start_index"": {model.StartIndex},
                ""end_index"": {model.EndIndex},
                ""average_cadence"": {model.AverageCadence},
                ""device_watts"": {model.DeviceWatts.ToLowerString()},
                ""average_watts"": {model.AverageWatts},
                ""average_heartrate"": {model.AverageHeartrate},
                ""max_heartrate"": {model.MaxHeartrate},
                ""pr_rank"": { model.PrRank},
                ""kom_rank"": {model.KomRank}
            }}";
    }

    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this IEnumerable<SegmentEffortDetailedModel> list)
    {
        return @$"[
                {string.Join(", ", list.Select(x => x.ToJson()))}
            ]";
    }
}
