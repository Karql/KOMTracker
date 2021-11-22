namespace KomTracker.Domain.Entities.Segment;

public class KomsSummarySegmentEffortEntity
{
    public int KomSummaryId { get; set; }
    public long SegmentEffortId { get; set; }
    public bool Kom { get; set; }
    public bool NewKom { get; set; }
    public bool ImprovedKom { get; set; }
    public bool LostKom { get; set; }
    public KomsSummaryEntity KomsSummary { get; set; }
    public SegmentEffortEntity SegmentEffort { get; set; }
}
