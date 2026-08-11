using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Gear;

/// <summary>Strava "DetailedGear" from GET /gear/{id}. Adds make/model/frame/weight over the summary.</summary>
public class GearDetailedModel : GearSummaryModel
{
    [JsonPropertyName("brand_name")]
    public string BrandName { get; set; }

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; }

    /// <summary>Bike frame type (1=mtb, 2=cross, 3=road, 4=time trial). Integer — small int→Bike.Type map lives in Phase 1c.</summary>
    [JsonPropertyName("frame_type")]
    public int FrameType { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>Gear weight in kilograms (can seed Bike.WeightKg in Phase 1c).</summary>
    [JsonPropertyName("weight")]
    public float Weight { get; set; }
}
