using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Segment;
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

    #region Detailed fields
    public float? TotalElevationGain { get; set; }
    public int? EffortCount { get; set; }
    public int? AthleteCount { get; set; }
    public int? StarCount { get; set; }

    public string MapPolyline { get; set; }
    #endregion

    [JsonIgnore]
    public ExtendedCategoryEnum ExtendedCategory => SegmentHelper.GetExtendedCategory(ClimbCategory, AverageGrade, Distance);

    [JsonIgnore]
    public string ExtendedCategoryText => SegmentHelper.GetExtendedCategoryText(ExtendedCategory);
}