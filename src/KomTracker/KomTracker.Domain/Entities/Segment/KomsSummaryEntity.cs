using System;
using System.Collections.Generic;

namespace KomTracker.Domain.Entities.Segment;

public class KomsSummaryEntity
{
    public int Id { get; set; }
    public DateTime TrackDate { get; set; }
    public int AthleteId { get; set; }
    public int Koms { get; set; }
    public int NewKoms { get; set; }
    public int ImprovedKoms { get; set; }
    public int LostKoms { get; set; }
    public List<SegmentEffortEntity> SegmentEfforts { get; set; }
}
