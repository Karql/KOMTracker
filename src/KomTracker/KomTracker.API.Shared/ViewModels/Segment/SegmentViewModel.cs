namespace KomTracker.API.Shared.ViewModels.Segment;

public class SegmentViewModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string ActivityType { get; set; }
    public float Distance { get; set; }
    public float AverageGrade { get; set; }
    public float MaximumGrade { get; set; }
    public float ElevationHigh { get; set; }
    public float ElevationLow { get; set; }
    public int ClimbCategory { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}