using System.Text.Json;
using System.Text.Json.Serialization;

namespace KomTracker.API.Shared.ViewModels.Strava;

/// <summary>The Strava webhook push payload (https://developers.strava.com/docs/webhooks/). Snake_case on the wire.</summary>
public class StravaWebhookEventViewModel
{
    [JsonPropertyName("object_type")]
    public string? ObjectType { get; set; }

    [JsonPropertyName("object_id")]
    public long ObjectId { get; set; }

    [JsonPropertyName("aspect_type")]
    public string? AspectType { get; set; }

    /// <summary>Free-form hash (update / deauthorization events); captured raw.</summary>
    [JsonPropertyName("updates")]
    public JsonElement? Updates { get; set; }

    [JsonPropertyName("owner_id")]
    public long OwnerId { get; set; }

    [JsonPropertyName("subscription_id")]
    public int SubscriptionId { get; set; }

    [JsonPropertyName("event_time")]
    public long EventTime { get; set; }
}
