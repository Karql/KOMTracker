using KomTracker.Domain.Contracts;
using System;

namespace KomTracker.Domain.Entities.Segment;

public class SegmentEntity : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string ActivityType { get; set; }
    public float Distance { get; set; }
    public float AverageGrade { get; set; }
    public float MaximumGrade { get; set; }
    public float ElevationHigh { get; set; }
    public float ElevationLow { get; set; }
    public float StartLatitude { get; set; }
    public float StartLongitude { get; set; }
    public float EndLatitude { get; set; }
    public float EndLongitude { get; set; }
    public int ClimbCategory { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public bool Private { get; set; }
    public bool Hazardous { get; set; }
    public bool Starred { get; set; }

    #region Detailed fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public float? TotalElevationGain { get; set; }
    public int? EffortCount { get; set; }
    public int? AthleteCount { get; set; }
    public int? StarCount { get; set; }
    public string MapPolyline { get; set; }
    #endregion
}
