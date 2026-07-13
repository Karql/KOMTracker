namespace KomTracker.Application.Shared.Models.Difficulty;

/// <summary>
/// A KOM difficulty/effort rating on the shared Coggan scale. Both "The Bar" (estimated from the
/// KOM time) and "The Burn" (measured from the holder's power) produce this shape. A <c>null</c>
/// result (rather than an instance) means "not rated" (descent, ultra-short, or no power/weight).
/// </summary>
public class KomRankResult
{
    /// <summary>Raw position between the low and high reference curves: (wKg - low) / (high - low).</summary>
    public double Level { get; set; }

    /// <summary>World Ranking 0-100 = clamp(Level, 0, 1) * 100. The primary, sortable number.</summary>
    public int Ranking { get; set; }

    /// <summary>Position 0-100 within the assigned category band (Sauce's "category ranking").</summary>
    public int CategoryRanking { get; set; }

    /// <summary>The Coggan category this effort maps to.</summary>
    public KomCategory Category { get; set; }

    /// <summary>Watts per kilogram used for the rating (estimated or measured).</summary>
    public double WKg { get; set; }

    /// <summary>Absolute watts (estimated for a reference rider, or measured).</summary>
    public double Watts { get; set; }

    /// <summary>Lower W/kg bound of the assigned category at this effort's duration.</summary>
    public double CategoryMinWKg { get; set; }

    /// <summary>Upper W/kg bound of the assigned category at this effort's duration
    /// (for World Class this is the top reference curve; efforts can exceed it).</summary>
    public double CategoryMaxWKg { get; set; }
}
