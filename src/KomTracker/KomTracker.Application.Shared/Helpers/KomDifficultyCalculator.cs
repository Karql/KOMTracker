using KomTracker.Application.Shared.Models.Difficulty;

namespace KomTracker.Application.Shared.Helpers;

/// <summary>
/// Computes the two KOM indicators on the shared Coggan scale (see <see cref="CogganRank"/>):
/// "The Bar" (difficulty estimated from the KOM time + terrain, for a reference rider) and
/// "The Burn" (effort measured from the holder's power). Both return <c>null</c> when not rateable.
/// </summary>
public static class KomDifficultyCalculator
{
    // Physics constants for the reference rider (tunable in one place; calibrate against real data).
    private const double G = 9.81;              // gravity m/s^2
    private const double Rho = 1.225;           // air density kg/m^3
    private const double Crr = 0.005;           // rolling resistance
    private const double CdA = 0.32;            // drag area m^2
    private const double DrivetrainEff = 0.97;  // drivetrain efficiency
    private const double RiderMassKg = 70.0;    // reference body mass (for W/kg)
    private const double BikeMassKg = 8.0;      // added for the resistive forces
    private const int MinDurationSeconds = 10;  // below this GPS/time noise dominates
    private const double MinGradePercent = -3;  // steeper descents are gravity-driven, not power-limited
                                                // (moderate descents are still rated; the P<=0 guard also catches coast-downs)

    /// <summary>
    /// "The Bar" — estimate the power a reference rider would need to match the KOM time over the
    /// segment, then rank it. Steady-speed model using the NET average grade for gravity: this is the
    /// correct energy balance for average power. (Using total_elevation_gain would double-count the
    /// climbing on rolling/net-flat segments and massively inflate the estimate, because it ignores the
    /// energy given back on the descents.) Returns null for descents, ultra-short segments, or non-positive power.
    /// </summary>
    public static KomRankResult? EstimateDifficulty(string? activityType, int elapsedSeconds,
        float distanceMeters, float averageGradePercent, string? sex)
    {
        if (!IsCycling(activityType)) return null;
        if (elapsedSeconds < MinDurationSeconds || distanceMeters <= 0) return null;
        if (averageGradePercent < MinGradePercent) return null;

        var totalMass = RiderMassKg + BikeMassKg;
        var v = distanceMeters / (double)elapsedSeconds;                       // m/s
        var theta = Math.Atan(averageGradePercent / 100.0);

        var gravity = totalMass * G * Math.Sin(theta) * v;                     // W (signed: assists on descents)
        var rolling = Crr * totalMass * G * Math.Cos(theta) * v;               // W
        var aero = 0.5 * Rho * CdA * v * v * v;                                // W
        var power = (gravity + rolling + aero) / DrivetrainEff;
        if (power <= 0) return null;

        var wKg = power / RiderMassKg;
        return CogganRank.Compute(wKg, elapsedSeconds, IsFemale(sex), power);
    }

    /// <summary>
    /// "The Burn" — rank the holder's actual measured effort. Requires a real power meter
    /// (<paramref name="deviceWatts"/>) and a known body weight; returns null otherwise.
    /// </summary>
    public static KomRankResult? MeasuredEffort(string? activityType, float? averageWatts, bool deviceWatts,
        float weight, int elapsedSeconds, string? sex)
    {
        if (!IsCycling(activityType)) return null;
        if (!deviceWatts || averageWatts is null || averageWatts <= 0
            || weight <= 0 || elapsedSeconds < MinDurationSeconds)
        {
            return null;
        }

        var wKg = averageWatts.Value / weight;
        return CogganRank.Compute(wKg, elapsedSeconds, IsFemale(sex), averageWatts.Value);
    }

    private static bool IsFemale(string? sex) => string.Equals(sex, "F", StringComparison.OrdinalIgnoreCase);

    // The physics/power model is cycling-specific; ratings are meaningless for run/walk/hike/etc.
    private static bool IsCycling(string? activityType) =>
        string.Equals(activityType, ActivityTypeConsts.Ride, StringComparison.OrdinalIgnoreCase);
}
