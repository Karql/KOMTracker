using KomTracker.Application.Shared.Models.Difficulty;

namespace KomTracker.API.Shared.ViewModels.Segment;

public class EffortViewModel
{
    public SegmentEffortViewModel SegmentEffort { get; set; }

    public KomsSummarySegmentEffortViewModel SummarySegmentEffort { get; set; }

    public SegmentViewModel Segment { get; set; }

    /// <summary>"The Bar" — KOM difficulty (estimated). Null when not rated.</summary>
    public KomRankResult? Bar { get; set; }

    /// <summary>"The Burn" — measured effort of the holder. Null when not rated.</summary>
    public KomRankResult? Burn { get; set; }
}
