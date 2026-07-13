using KomTracker.Application.Shared.Models.Difficulty;
using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Segment;

public class EffortModel
{
    public SegmentEffortEntity SegmentEffort { get; set; }

    public KomsSummarySegmentEffortEntity SummarySegmentEffort { get; set; }

    public SegmentEntity? Segment { get; set; }

    /// <summary>"The Bar" — difficulty estimated from the KOM time (null when not rateable).</summary>
    public KomRankResult? Bar { get; set; }

    /// <summary>"The Burn" — effort measured from the holder's power (null when not rateable).</summary>
    public KomRankResult? Burn { get; set; }
}