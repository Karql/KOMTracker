using FluentAssertions;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Queries.KomTakeover;
using KomTracker.Domain.Entities.Athlete;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KomTracker.Application.Tests.Queries.KomTakeover;

public class GetKomTakeoverPairsQueryTests
{
    private static KomTakeoverCountModel Count(int takenBy, int lostBy, int count)
        => new() { TakenByAthleteId = takenBy, LostByAthleteId = lostBy, Count = count };

    private static IReadOnlyDictionary<int, AthleteEntity> Athletes(params int[] ids)
        => ids.ToDictionary(id => id, id => new AthleteEntity { AthleteId = id });

    [Fact]
    public void Orient_pairs_puts_higher_count_athlete_on_the_left()
    {
        var counts = new[] { Count(1, 2, 5), Count(2, 1, 3) };

        var pairs = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1, 2)).ToList();

        pairs.Should().ContainSingle();
        var p = pairs.Single();
        p.WinnerAthlete.AthleteId.Should().Be(1);
        p.WinnerKoms.Should().Be(5);
        p.LoserAthlete.AthleteId.Should().Be(2);
        p.LoserKoms.Should().Be(3);
    }

    [Fact]
    public void Orient_pairs_winner_can_be_the_higher_id()
    {
        var counts = new[] { Count(1, 2, 2), Count(2, 1, 6) };

        var p = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1, 2)).Single();

        p.WinnerAthlete.AthleteId.Should().Be(2);
        p.WinnerKoms.Should().Be(6);
        p.LoserAthlete.AthleteId.Should().Be(1);
        p.LoserKoms.Should().Be(2);
    }

    [Fact]
    public void Orient_pairs_tie_puts_lower_id_on_the_left()
    {
        var counts = new[] { Count(2, 1, 4), Count(1, 2, 4) };

        var p = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1, 2)).Single();

        p.WinnerAthlete.AthleteId.Should().Be(1);
        p.WinnerKoms.Should().Be(4);
        p.LoserAthlete.AthleteId.Should().Be(2);
        p.LoserKoms.Should().Be(4);
    }

    [Fact]
    public void Orient_pairs_handles_one_directional()
    {
        var counts = new[] { Count(1, 2, 3) };

        var p = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1, 2)).Single();

        p.WinnerAthlete.AthleteId.Should().Be(1);
        p.WinnerKoms.Should().Be(3);
        p.LoserAthlete.AthleteId.Should().Be(2);
        p.LoserKoms.Should().Be(0);
    }

    [Fact]
    public void Orient_pairs_orders_by_total_takeovers_desc()
    {
        var counts = new[]
        {
            Count(1, 2, 1), Count(2, 1, 1), // total 2
            Count(3, 4, 5), Count(4, 3, 4), // total 9
        };

        var pairs = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1, 2, 3, 4)).ToList();

        pairs.Should().HaveCount(2);
        (pairs[0].WinnerKoms + pairs[0].LoserKoms).Should().Be(9);
        (pairs[1].WinnerKoms + pairs[1].LoserKoms).Should().Be(2);
    }

    [Fact]
    public void Orient_pairs_skips_pair_when_athlete_missing()
    {
        var counts = new[] { Count(1, 99, 3) };

        var pairs = GetKomTakeoverPairsQueryHandler.OrientPairs(counts, Athletes(1)).ToList();

        pairs.Should().BeEmpty();
    }
}
