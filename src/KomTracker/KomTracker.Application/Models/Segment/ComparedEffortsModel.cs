using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Linq;

namespace KomTracker.Application.Models.Segment;

public class ComparedEffortsModel
{
    public List<SegmentEffortEntity> Koms { get; set; } = new List<SegmentEffortEntity>();
    public List<SegmentEffortEntity> NewKoms { get; set; } = new List<SegmentEffortEntity>();
    public List<SegmentEffortEntity> LostKoms { get; set; } = new List<SegmentEffortEntity>();
    public List<SegmentEffortEntity> ImprovedKoms { get; set; } = new List<SegmentEffortEntity>();

    public bool AnyChanges => NewKoms.Any() || ImprovedKoms.Any() || LostKoms.Any();
}
