using KomTracker.Application.Shared.Models.Difficulty;

namespace KomTracker.Application.Shared.Helpers;

/// <summary>
/// Continuous power-duration ranking based on Andy Coggan's power profile.
/// Ported from Sauce4Strava (MIT, https://github.com/SauceLLC/sauce4strava, src/common/lib.js).
/// Credit: Sauce4Strava contributors and Dr. Andy Coggan. The Normalized-Power blending Sauce
/// applies to long efforts is intentionally omitted (we have no per-second power stream); the
/// >1h endurance decay is part of the curve and is kept.
/// </summary>
public static class CogganRank
{
    private readonly record struct Curve(double SlopeFactor, double SlopePeriod, double SlopeAdjust, double SlopeOffset, double BaseOffset);

    // Verbatim from Sauce's rankConstants.
    private static readonly Curve MaleHigh = new(2.82, 2500, 1.4, 3.6, 6.08);
    private static readonly Curve MaleLow = new(2, 3000, 1.3, 1, 1.74);
    private static readonly Curve FemaleHigh = new(2.65, 2500, 1, 3.6, 5.39);
    private static readonly Curve FemaleLow = new(2.15, 300, 6, 1.5, 1.4);

    // Category cut-offs on Level (Sauce's rankLevels), descending.
    private static readonly (double Requirement, KomCategory Category)[] Levels =
    [
        (7d / 8d, KomCategory.WorldClass),
        (6d / 8d, KomCategory.Pro),
        (5d / 8d, KomCategory.Cat1),
        (4d / 8d, KomCategory.Cat2),
        (3d / 8d, KomCategory.Cat3),
        (2d / 8d, KomCategory.Cat4),
        (1d / 8d, KomCategory.Cat5),
        (double.NegativeInfinity, KomCategory.Recreational),
    ];

    /// <summary>
    /// Rate a watts-per-kilogram effort of a given duration against the reference curves.
    /// <paramref name="watts"/> is stored on the result for display only.
    /// </summary>
    public static KomRankResult Compute(double wKg, double durationSeconds, bool female, double watts)
    {
        var high = Scaler(durationSeconds, female ? FemaleHigh : MaleHigh);
        var low = Scaler(durationSeconds, female ? FemaleLow : MaleLow);
        var level = (wKg - low) / (high - low);

        var (category, categoryLevel, lower, upper) = MapLevel(level);

        return new KomRankResult
        {
            Level = level,
            // Not capped at 100 (only floored at 0): scores can exceed 100 to show how far beyond
            // World Class a (usually aided) time is. Matches Sauce, which doesn't clamp level*100.
            Ranking = (int)Math.Round(Math.Max(0, level) * 100),
            CategoryRanking = (int)Math.Round(Math.Clamp(categoryLevel, 0, 1) * 100),
            Category = category,
            WKg = wKg,
            Watts = watts,
            CategoryMinWKg = low + (lower * (high - low)),
            CategoryMaxWKg = low + (upper * (high - low)),
        };
    }

    private static double Scaler(double duration, Curve c)
    {
        var t = (c.SlopePeriod / duration) * c.SlopeAdjust;
        var slope = Math.Log10(t + c.SlopeOffset);
        var wKgDiff = Math.Pow(slope, c.SlopeFactor);
        var enduro = duration > 3600 ? 1.0 / ((Math.Log(duration / 3600.0) * 0.1) + 1.0) : 1.0;
        return (wKgDiff + c.BaseOffset) * enduro;
    }

    // Returns the category, the position within its band (0-1), and the band's level bounds
    // [lower, upper] (lower is 0 for the open-ended Recreational band).
    private static (KomCategory Category, double CategoryLevel, double Lower, double Upper) MapLevel(double level)
    {
        var lastRequirement = 1.0;
        foreach (var (requirement, category) in Levels)
        {
            if (level >= requirement)
            {
                // Recreational is the open-ended bottom band (requirement = -inf): measure position
                // from 0 up to the Cat 5 threshold instead of dividing by an infinite span.
                var lower = double.IsNegativeInfinity(requirement) ? 0.0 : requirement;
                var categoryLevel = double.IsNegativeInfinity(requirement)
                    ? level / lastRequirement
                    : (level - requirement) / (lastRequirement - requirement);
                return (category, categoryLevel, lower, lastRequirement);
            }
            lastRequirement = requirement;
        }
        return (KomCategory.Recreational, 0, 0, 1.0 / 8.0);
    }
}
