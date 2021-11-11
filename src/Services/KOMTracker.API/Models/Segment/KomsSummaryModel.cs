using System;
using System.Collections.Generic;

namespace KOMTracker.API.Models.Segment;

public class KomsSummaryModel
{
    public int Id { get; set; }
    public DateTime TrackDate { get; set; }
    public int AthleteId { get; set; }
    public int Koms { get; set; }
    public int NewKoms { get; set; }
    public int LostKoms { get; set; }
    public List<SegmentEffortModel> SegmentEfforts { get; set; }
}
