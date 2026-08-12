using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Base;
public class PolylineMapModel
{
    [JsonPropertyName("polyline")]
    public string Polyline { get; set; }

    /// <summary>Present on the list endpoints (e.g. activities) instead of the full <see cref="Polyline"/>.</summary>
    [JsonPropertyName("summary_polyline")]
    public string SummaryPolyline { get; set; }
}
