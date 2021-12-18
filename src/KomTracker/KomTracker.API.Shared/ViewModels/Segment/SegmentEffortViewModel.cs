using System.Text.Json.Serialization;

namespace KomTracker.API.Shared.ViewModels.Segment;

public class SegmentEffortViewModel
{
    public long Id { get; set; }
    public long SegmentId { get; set; }
    public string Name { get; set; } = default!;
    public int ElapsedTime { get; set; }
    public int MovingTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime StartDateLocal { get; set; }
    public float Distance { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public float? AverageCadence { get; set; }
    public bool DeviceWatts { get; set; }
    public float? AverageWatts { get; set; }
    public float? AverageHeartrate { get; set; }
    public float? MaxHeartrate { get; set; }

    /// <summary>
    /// Speed in km/h
    /// </summary>
    [JsonIgnore]
    public float Speed => (Distance / ElapsedTime) * 3.6f; 
}