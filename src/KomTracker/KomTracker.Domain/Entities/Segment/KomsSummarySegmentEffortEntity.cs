using KomTracker.Domain.Contracts;
using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Segment;

public class KomsSummarySegmentEffortEntity : BaseEntity
{
    public int KomSummaryId { get; set; }
    public long SegmentEffortId { get; set; }
    public bool Kom { get; set; }
    public bool NewKom { get; set; }
    public bool ImprovedKom { get; set; }
    public bool LostKom { get; set; }
    public bool ReturnedKom { get; set; }
    [JsonIgnore]
    public KomsSummaryEntity KomsSummary { get; set; }
    [JsonIgnore]
    public SegmentEffortEntity SegmentEffort { get; set; }
}
