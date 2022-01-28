using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Segment;

public static class SegmentSummaryModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this SegmentSummaryModel model)
    {
        return @$"{{
            {GetPropertiesJson(model)}
        }}";
    }

    internal static string GetPropertiesJson(SegmentSummaryModel model)
    {
        return @$"
            ""resource_state"": {(int)model.ResourceState},
            ""id"": {model.Id},
            ""name"": ""{model.Name}"",
            ""activity_type"": ""{model.ActivityType}"",
            ""distance"": {model.Distance},
            ""average_grade"": {model.AverageGrade},
            ""maximum_grade"": {model.MaximumGrade},
            ""elevation_high"": {model.ElevationHigh},
            ""elevation_low"": {model.ElevationLow},
            ""start_latitude"": {model.StartLatitude},
            ""start_longitude"": {model.StartLongitude},
            ""end_latitude"": {model.EndLatitude},
            ""end_longitude"": {model.EndLongitude},
            ""climb_category"": {model.ClimbCategory},
            ""city"": ""{model.City}"",
            ""country"": ""{model.Country}"",
            ""private"": {model.Private.ToLowerString()},
            ""hazardous"": {model.Hazardous.ToLowerString()},
            ""starred"": {model.Starred.ToLowerString()}
        ";
    }
}
