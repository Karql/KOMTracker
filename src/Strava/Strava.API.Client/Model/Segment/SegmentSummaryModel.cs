using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment;

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

    [JsonPropertyName("start_latlng")]
    public float[] StartLatLng { get; set; } = new float[2];

    [JsonPropertyName("end_latlng")]
    public float[] EndLatLng { get; set; } = new float[2];

    // Those fields are not documented
    // but it's easier to convert from them
    // Edit 2022-04-04: Strava has started send nulls...
    //[JsonPropertyName("start_latitude")]
    [JsonIgnore]
    public float StartLatitude
    {
        get
        {
            return StartLatLng[0];
        }
        set
        {
            StartLatLng[0] = value;
        }
    }

    //[JsonPropertyName("start_longitude")]
    [JsonIgnore]
    public float StartLongitude
    {
        get
        {
            return StartLatLng[1];
        }
        set
        {
            StartLatLng[1] = value;
        }
    }

    //[JsonPropertyName("end_latitude")]
    [JsonIgnore]
    public float EndLatitude
    {
        get
        {
            return EndLatLng[0];
        }
        set
        {
            EndLatLng[0] = value;
        }
    }

    //[JsonPropertyName("end_longitude")]
    [JsonIgnore]
    public float EndLongitude
    {
        get
        {
            return EndLatLng[1];
        }
        set
        {
            EndLatLng[1] = value;
        }
    }

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
