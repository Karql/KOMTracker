using FluentAssertions;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Difficulty;
using Xunit;

namespace KomTracker.Application.Tests.Helpers;

public class KomDifficultyCalculatorTests
{
    // --- CogganRank (port cross-check vs Sauce numbers) ---

    [Fact]
    public void CogganRank_male_4_5_wkg_over_1h_is_cat2()
    {
        // male 1h reference curve: high ~= 6.39, low ~= 1.84 -> level ~= 0.58
        var res = CogganRank.Compute(4.5, 3600, female: false, watts: 315);

        res.Category.Should().Be(KomCategory.Cat2);
        res.Ranking.Should().BeInRange(55, 61);
        res.Watts.Should().Be(315);
    }

    [Fact]
    public void CogganRank_at_high_curve_is_world_class_100()
    {
        var res = CogganRank.Compute(6.39, 3600, female: false, watts: 0);

        res.Category.Should().Be(KomCategory.WorldClass);
        res.Ranking.Should().Be(100);
    }

    [Fact]
    public void CogganRank_beyond_world_class_is_not_capped_at_100()
    {
        // Freakishly high w/kg (e.g. an aided flat) should rank well above 100, still World Class.
        var res = CogganRank.Compute(15, 3600, female: false, watts: 0);

        res.Category.Should().Be(KomCategory.WorldClass);
        res.Ranking.Should().BeGreaterThan(100);
    }

    [Fact]
    public void CogganRank_below_low_curve_is_recreational_zero()
    {
        var res = CogganRank.Compute(1.0, 3600, female: false, watts: 0);

        res.Category.Should().Be(KomCategory.Recreational);
        res.Ranking.Should().Be(0);
    }

    // --- The Bar (estimated difficulty) ---

    [Fact]
    public void EstimateDifficulty_ranks_a_hard_remote_effort_above_an_easy_flat_one()
    {
        // Remote steep: 400 m @ 12% in 60 s. Flat boulevard: 800 m flat in 60 s.
        var remote = KomDifficultyCalculator.EstimateDifficulty("Ride",60, 400, 12f, "M");
        var flat = KomDifficultyCalculator.EstimateDifficulty("Ride",60, 800, 0f, "M");

        remote.Should().NotBeNull();
        flat.Should().NotBeNull();
        remote!.Ranking.Should().BeGreaterThan(flat!.Ranking);
    }

    [Fact]
    public void EstimateDifficulty_does_not_inflate_a_fast_net_flat_effort()
    {
        // Grajów - Górki: 3892 m, net -0.4%, 323 s (43.4 km/h). Real measured power was ~315 W;
        // a neutral solo estimate should be in the same ballpark (~350-420 W), NOT ~560 W, and
        // nowhere near World Class.
        var res = KomDifficultyCalculator.EstimateDifficulty("Ride",323, 3892, -0.4f, "M");

        res.Should().NotBeNull();
        res!.Watts.Should().BeInRange(320, 430);
        res.Category.Should().BeOneOf(KomCategory.Cat3, KomCategory.Cat2);
    }

    [Fact]
    public void EstimateDifficulty_returns_null_for_a_descent()
    {
        var res = KomDifficultyCalculator.EstimateDifficulty("Ride",60, 1000, -5f, "M");

        res.Should().BeNull();
    }

    [Fact]
    public void EstimateDifficulty_returns_null_for_ultra_short_segments()
    {
        var res = KomDifficultyCalculator.EstimateDifficulty("Ride",5, 60, 3f, "M");

        res.Should().BeNull();
    }

    [Theory]
    [InlineData("Run")]
    [InlineData("Walk")]
    [InlineData("Hike")]
    public void Ratings_are_only_for_cycling(string activityType)
    {
        KomDifficultyCalculator.EstimateDifficulty(activityType, 120, 500, 8f, "M").Should().BeNull();
        KomDifficultyCalculator.MeasuredEffort(activityType, 300f, deviceWatts: true, weight: 70f, elapsedSeconds: 1200, "M").Should().BeNull();
    }

    // --- The Burn (measured effort) ---

    [Fact]
    public void MeasuredEffort_rates_a_real_power_meter_effort()
    {
        var res = KomDifficultyCalculator.MeasuredEffort("Ride",300f, deviceWatts: true, weight: 70f, elapsedSeconds: 1200, "M");

        res.Should().NotBeNull();
        res!.WKg.Should().BeApproximately(300f / 70f, 0.01);
        res.Watts.Should().Be(300);
    }

    [Fact]
    public void MeasuredEffort_returns_null_without_a_power_meter()
    {
        var res = KomDifficultyCalculator.MeasuredEffort("Ride",300f, deviceWatts: false, weight: 70f, elapsedSeconds: 1200, "M");

        res.Should().BeNull();
    }

    [Fact]
    public void MeasuredEffort_returns_null_without_weight()
    {
        var res = KomDifficultyCalculator.MeasuredEffort("Ride",300f, deviceWatts: true, weight: 0f, elapsedSeconds: 1200, "M");

        res.Should().BeNull();
    }

    [Fact]
    public void MeasuredEffort_returns_null_without_watts()
    {
        var res = KomDifficultyCalculator.MeasuredEffort("Ride",null, deviceWatts: true, weight: 70f, elapsedSeconds: 1200, "M");

        res.Should().BeNull();
    }
}
