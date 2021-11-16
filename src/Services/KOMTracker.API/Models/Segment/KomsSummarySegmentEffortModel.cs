namespace KOMTracker.API.Models.Segment;

public class KomsSummarySegmentEffortModel
{
    public int KomSummaryId { get; set; }
    public long SegmentEffortId { get; set; }
    public KomsSummaryModel KomsSummary { get; set; }
    public SegmentEffortModel SegmentEffort { get; set; }
}
