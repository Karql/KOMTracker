using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Shared.ViewModels.Segment;

public class KomsSummarySegmentEffortViewModel
{
    public int KomSummaryId { get; set; }
    public long SegmentEffortId { get; set; }
    public bool Kom { get; set; }
    public bool NewKom { get; set; }
    public bool ImprovedKom { get; set; }
    public bool LostKom { get; set; }
    public bool ReturnedKom { get; set; }
}
