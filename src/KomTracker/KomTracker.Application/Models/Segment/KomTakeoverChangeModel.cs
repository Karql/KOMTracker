using System;

namespace KomTracker.Application.Models.Segment;

public enum KomChangeTypeEnum
{
    New,
    Lost,
    Returned
}

/// <summary>
/// A KOM change relevant to takeover detection (one koms_summary_segment_effort row),
/// flattened with its athlete (sex), segment and summary so the resolver can work in-memory.
/// </summary>
public class KomTakeoverChangeModel
{
    public int AthleteId { get; set; }
    public string? Sex { get; set; }
    public long SegmentId { get; set; }
    public long SegmentEffortId { get; set; }
    public int KomsSummaryId { get; set; }
    public DateTime TrackDate { get; set; }
    public KomChangeTypeEnum ChangeType { get; set; }
}
