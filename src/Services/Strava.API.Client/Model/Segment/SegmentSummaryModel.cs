using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment
{
    public class SegmentSummaryModel
    {
        [JsonPropertyName("resource_state")]
        public ResourceStateEnum ResourceState { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("activity_type")]
        public string ActivityType { get; set; }

        [JsonPropertyName("distance")]
        public float Distance { get; set; }

        [JsonPropertyName("average_grade")]
        public float AverageGrade { get; set; }

        [JsonPropertyName("maximum_grade")]
        public float MaximumGrade { get; set; }

        [JsonPropertyName("elevation_high")]
        public float ElevationHigh { get; set; }

        [JsonPropertyName("elevation_low")]
        public float ElevationLow { get; set; }

        // Those fields are not documented
        // but it's easier to convert from them
        [JsonPropertyName("start_latitude")]
        public float StartLatitude { get; set; }

        [JsonPropertyName("start_longitude")]
        public float StartLongitude { get; set; }

        [JsonPropertyName("end_latitude")]
        public float EndLatitude { get; set; }

        [JsonPropertyName("end_longitude")]
        public float EndLongitude { get; set; }

        [JsonPropertyName("climb_category")]
        public int ClimbCategory { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("hazardous")]
        public bool Hazardous { get; set; }

        [JsonPropertyName("starred")]
        public bool Starred { get; set; }
    }
}
