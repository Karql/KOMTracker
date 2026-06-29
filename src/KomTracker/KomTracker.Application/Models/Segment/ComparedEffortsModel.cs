using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Linq;

namespace KomTracker.Application.Models.Segment;

public class ComparedEffortsModel
{
    // Safeguard thresholds to skip updates when Strava API returns suspicious (empty/partial) data.
    private const int EmptyResponseMaxLostKoms = 20;
    private const int PartialResponseMaxLostKoms = 50;
    private const double PartialResponseMaxLostRatio = 0.35;

    public bool FirstCompare { get; set; } = false;
    public int KomsCount { get; set; } = 0;
    public int NewKomsCount { get; set; } = 0;
    public int ImprovedKomsCount { get; set; } = 0;
    public int LostKomsCount { get; set; } = 0;
    public int ReturnedKomsCount { get; set; } = 0;

    /// <summary>
    /// Number of KOMs from the previous fetch. Set by SegmentService.CompareEfforts.
    /// </summary>
    public int PreviousKomsCount { get; set; } = 0;

    public List<EffortModel> Efforts { get; set; } = new List<EffortModel>();

    public bool AnyChanges => NewKomsCount > 0 || ImprovedKomsCount > 0 || LostKomsCount > 0;

    /// <summary>
    /// Detects suspicious Strava API responses that should not be persisted.
    /// Strava occasionally returns an empty or partial KOMs list (HTTP 200, no error),
    /// which would otherwise wipe a user's KOMs only to restore them on the next run.
    /// </summary>
    public bool IsSuspiciousApiResponse =>
        // Empty list response
        (KomsCount == 0 && LostKomsCount > EmptyResponseMaxLostKoms)
        // Partial list response (API truncation)
        || (KomsCount > 0
            && LostKomsCount > PartialResponseMaxLostKoms
            && (double)LostKomsCount / PreviousKomsCount > PartialResponseMaxLostRatio);
}
