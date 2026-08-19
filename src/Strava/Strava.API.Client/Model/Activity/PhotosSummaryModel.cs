using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>Photo summary (from DetailedActivity `photos`).</summary>
public class PhotosSummaryModel
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("primary")]
    public PrimaryPhotoModel Primary { get; set; }
}

/// <summary>The primary photo of an activity (DetailedActivity `photos.primary`).</summary>
public class PrimaryPhotoModel
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; }

    /// <summary>Map of size (e.g. "100", "600") → image URL.</summary>
    [JsonPropertyName("urls")]
    public Dictionary<string, string> Urls { get; set; }

    [JsonPropertyName("source")]
    public int Source { get; set; }

    [JsonPropertyName("media_type")]
    public int? MediaType { get; set; }
}
