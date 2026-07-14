using KomTracker.Application.Models.Segment;
using KomTracker.Application.Shared.Helpers;

namespace KomTracker.Application.Services;

/// <summary>
/// Sets the KOM difficulty/effort ratings (The Bar / The Burn) on an <see cref="EffortModel"/>.
/// The Bar is estimated from the time + terrain (athlete-independent); The Burn needs the effort
/// holder's sex/weight. Shared by the koms, koms-changes and takeover queries.
/// </summary>
public static class KomRatingEnricher
{
    public static void Apply(EffortModel effort, string? sex, float weight)
    {
        if (effort.Segment is null)
        {
            return;
        }

        effort.Bar = KomDifficultyCalculator.EstimateDifficulty(
            effort.Segment.ActivityType,
            effort.SegmentEffort.ElapsedTime,
            effort.Segment.Distance,
            effort.Segment.AverageGrade,
            sex);

        effort.Burn = KomDifficultyCalculator.MeasuredEffort(
            effort.Segment.ActivityType,
            effort.SegmentEffort.AverageWatts,
            effort.SegmentEffort.DeviceWatts,
            weight,
            effort.SegmentEffort.ElapsedTime,
            sex);
    }
}
