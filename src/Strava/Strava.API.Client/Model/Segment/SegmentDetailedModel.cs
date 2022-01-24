using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment;

public  class SegmentDetailedModel : SegmentSummaryModel
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public float? TotalElevationGain { get; set; }
    
    [JsonPropertyName("effort_count")]
    public int EffortCount { get; set; }

    [JsonPropertyName("athlete_count")]
    public int AthleteCount { get; set; }

    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }

    /*
     * Not needed properties for now
     * map
     * athlete_segment_stats
     * xoms
     * local_legend
     */
}
