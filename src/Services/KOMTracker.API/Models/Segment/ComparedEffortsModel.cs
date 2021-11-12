using System.Collections.Generic;
using System.Linq;

namespace KOMTracker.API.Models.Segment;

public class ComparedEffortsModel
{
    public List<SegmentEffortModel> Koms { get; set; } = new List<SegmentEffortModel>();
    public List<SegmentEffortModel> NewKoms { get; set; } = new List<SegmentEffortModel>();
    public List<SegmentEffortModel> LostKoms { get; set; } = new List<SegmentEffortModel>();
    public List<SegmentEffortModel> ImprovedKoms { get; set; } = new List<SegmentEffortModel>();

    public bool AnyChanges => NewKoms.Any() || ImprovedKoms.Any() || LostKoms.Any();
}
