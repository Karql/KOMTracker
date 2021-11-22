using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Segment;

public class SegmentEffortWithLinkToKomsSummaryModel
{
    public SegmentEffortEntity SegmentEffort { get; set; }

    public KomsSummarySegmentEffortEntity Link { get; set; }
}
