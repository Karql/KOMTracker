using System.Text.Json.Serialization;

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

    [JsonIgnore]
    public int ClimbCategoryExtended
    {
        get
        {
            return AverageGrade < -4.0f ?
                -1
                : ClimbCategory;
        }
    }

    public string ClimbCategoryExtendedText => ClimbCategoryExtended switch
    {
        -1 => "DH",
        0 => "0",
        1 => "4",
        2 => "3",
        3 => "2",
        4 => "1",
        5 => "HC",
        _ => throw new ArgumentOutOfRangeException($"{nameof(ClimbCategoryExtended)} should has value between -1 and 5"),
    };
}