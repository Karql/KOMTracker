using KomTracker.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Segment;

public class KomsSummaryEntity : BaseEntity
{
    public int Id { get; set; }
    public DateTime TrackDate { get; set; }
    public int AthleteId { get; set; }
    public int Koms { get; set; }
    public int NewKoms { get; set; }
    public int ImprovedKoms { get; set; }
    public int LostKoms { get; set; }
    public int ReturnedKoms { get; set; }
    [JsonIgnore]
    public List<SegmentEffortEntity> SegmentEfforts { get; set; }
}
