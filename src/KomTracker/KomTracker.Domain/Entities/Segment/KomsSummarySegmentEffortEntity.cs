namespace KomTracker.Domain.Entities.Segment;

public class KomsSummarySegmentEffortEntity
{
    public int KomSummaryId { get; set; }
    public long SegmentEffortId { get; set; }
    public KomsSummaryEntity KomsSummary { get; set; }
    public SegmentEffortEntity SegmentEffort { get; set; }
}
