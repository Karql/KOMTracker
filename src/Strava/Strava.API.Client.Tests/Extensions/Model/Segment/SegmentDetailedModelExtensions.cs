using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Segment;

public static class SegmentDetailedModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this SegmentDetailedModel model)
    {
        return @$"{{
            {SegmentSummaryModelExtensions.GetPropertiesJson(model)},
            ""created_at"": ""{model.CreatedAt.ToUtcIso()}"",
            ""updated_at"": ""{model.UpdatedAt.ToUtcIso()}"",
            ""total_elevation_gain"": {model.TotalElevationGain},
            ""effort_count"": {model.EffortCount},
            ""athlete_count"": {model.AthleteCount},
            ""star_count"": {model.StarCount}
        }}";
    }
}
