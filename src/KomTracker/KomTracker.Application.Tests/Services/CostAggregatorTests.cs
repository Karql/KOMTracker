#nullable enable
using FluentAssertions;
using KomTracker.API.Shared.CostAnalysis;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.CostAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KomTracker.Application.Tests.Services;

public class CostAggregatorTests
{
    private static readonly DateTime Today = new(2026, 10, 20);

    private static CostLine Line(CostSource source, decimal amount, int? year, int? bikeId = 3, string group = "G", string category = "C",
        int itemId = 1, CostKind kind = CostKind.Cost)
        => new()
        {
            Source = source, Amount = amount, FullAmount = Math.Abs(amount), Date = year is int y ? new DateTime(y, 6, 1) : null,
            BikeId = bikeId, Group = group, Category = category, ItemId = itemId, ItemName = $"Item{itemId}", Kind = kind,
            ComponentCategory = source == CostSource.Components ? ComponentCategory.Other : null
        };

    private static CostAnalysisReport Report() => new()
    {
        Lines = new List<CostLine>
        {
            Line(CostSource.Bikes, 5000m, 2024, itemId: 3, group: "Bikes", category: "Bikes"),
            Line(CostSource.Bikes, -2000m, 2026, itemId: 3, group: "Bikes", category: "Bikes", kind: CostKind.Resale),
            Line(CostSource.Components, 300m, 2025, group: "Drivetrain", category: "Chain", itemId: 10),
            Line(CostSource.Components, 200m, 2025, group: "Drivetrain", category: "Cassette", itemId: 11),
            Line(CostSource.Components, 100m, 2025, bikeId: 4, group: "Wheels", category: "Tire", itemId: 12),
            Line(CostSource.Components, 80m, null, group: "Wheels", category: "Tire", itemId: 13),
            Line(CostSource.Expenses, 120m, 2025, bikeId: null, group: "Clothing", category: "Shoes", itemId: 20)
        },
        Bikes = new List<CostBike>
        {
            new() { Id = 3, Name = "Trutka", SeedKm = 1000m, KmByYear = new() { [2024] = 2000m, [2025] = 3000m } },
            new() { Id = 4, Name = "Focus", KmByYear = new() { [2025] = 1000m } }
        }
    };

    [Fact]
    public void All_time_net_subtracts_sales_and_gross_does_not()
    {
        var report = Report();

        CostAggregator.Kpis(report, new CostFilter(), Today).Net.Should().Be(3800m);
        var gross = CostAggregator.Kpis(report, new CostFilter { Net = false }, Today);
        gross.Net.Should().Be(5800m);
        gross.Recovered.Should().Be(2000m);
    }

    [Fact]
    public void A_year_drops_undated_lines_and_counts_only_that_years_km()
    {
        var kpis = CostAggregator.Kpis(Report(), new CostFilter { FromYear = 2025, ToYear = 2025 }, Today);

        kpis.Net.Should().Be(720m);          // 300 + 200 + 100 + 120 (undated 80 left out)
        kpis.Km.Should().Be(4000m);          // 3000 + 1000, no seed
        kpis.PerKm.Should().Be(0.18m);
        kpis.Years.Should().Be(1);
    }

    [Fact]
    public void Per_year_of_one_bike_counts_the_years_since_its_own_first_cost()
    {
        var report = Report();
        report.Lines.Add(Line(CostSource.Components, 600m, 2025, bikeId: 5, itemId: 30));
        report.Bikes.Add(new CostBike { Id = 5, Name = "New", KmByYear = new() { [2026] = 100m } });

        var kpis = CostAggregator.Kpis(report, new CostFilter { BikeId = 5 }, Today);

        kpis.Years.Should().Be(2);          // 2025 → 2026, not since the user's first cost in 2024
        kpis.PerYear.Should().Be(300m);
    }

    [Fact]
    public void A_retired_bike_ends_at_its_last_cost_or_ride_year()
    {
        var report = Report();
        report.Bikes[1].Lifecycle = Domain.Entities.Bike.BikeLifecycle.Sold;   // Focus: costs + km only in 2025

        CostAggregator.Kpis(report, new CostFilter { BikeId = 4 }, Today).Years.Should().Be(1);
    }

    [Fact]
    public void All_time_km_include_the_seed()
        => CostAggregator.Km(Report(), new CostFilter { BikeId = 3 }).Should().Be(6000m);

    [Fact]
    public void Bike_and_no_bike_scopes_partition_the_lines()
    {
        var report = Report();
        var all = CostAggregator.Kpis(report, new CostFilter(), Today).Net;
        var perBike = CostAggregator.Kpis(report, new CostFilter { BikeId = 3 }, Today).Net
                      + CostAggregator.Kpis(report, new CostFilter { BikeId = 4 }, Today).Net
                      + CostAggregator.Kpis(report, new CostFilter { BikeId = CostFilter.NoBike }, Today).Net;

        perBike.Should().Be(all);
        CostAggregator.Km(report, new CostFilter { BikeId = CostFilter.NoBike }).Should().Be(0m);
    }

    [Fact]
    public void Disabled_sources_are_left_out()
        => CostAggregator.Kpis(Report(), new CostFilter { Sources = new HashSet<CostSource> { CostSource.Components } }, Today)
            .Net.Should().Be(680m);

    [Fact]
    public void Breakdown_drills_from_sources_to_groups_categories_and_items()
    {
        var report = Report();
        var filter = new CostFilter { Net = false };

        CostAggregator.Breakdown(report, filter, new CostDrill()).Select(s => s.Key)
            .Should().Equal("Bikes", "Components", "Expenses");

        var groups = CostAggregator.Breakdown(report, filter, new CostDrill(CostSource.Components));
        groups.Select(s => (s.Label, s.Amount)).Should().Equal(("Drivetrain", 500m), ("Wheels", 180m));
        groups.Sum(s => s.Percent).Should().Be(100m);

        CostAggregator.Breakdown(report, filter, new CostDrill(CostSource.Components, "Drivetrain")).Select(s => s.Label)
            .Should().Equal("Chain", "Cassette");

        CostAggregator.Breakdown(report, filter, new CostDrill(CostSource.Components, "Wheels", "Tire")).Select(s => s.Label)
            .Should().Equal("Item12", "Item13");
    }

    [Fact]
    public void Timeline_is_per_year_or_per_month_within_one_year()
    {
        var report = Report();

        var years = CostAggregator.Timeline(report, new CostFilter(), new CostDrill());
        years.Periods.Should().Equal("2024", "2025", "2026");
        years.Series.Select(s => s.Key).Should().BeEquivalentTo("Bikes", "Components", "Expenses");

        var months = CostAggregator.Timeline(report, new CostFilter { FromYear = 2025, ToYear = 2025 }, new CostDrill());
        months.Periods.Should().HaveCount(12);
        months.Total(5).Should().Be(720m);
    }

    [Fact]
    public void Timeline_drills_into_groups_and_categories_of_a_source()
    {
        var report = Report();

        var groups = CostAggregator.Timeline(report, new CostFilter(), new CostDrill(CostSource.Components));
        groups.Series.Select(s => (s.Key, s.Total)).Should().Equal(("Drivetrain", 500m), ("Wheels", 100m));   // undated 80 has no year

        var categories = CostAggregator.Timeline(report, new CostFilter(), new CostDrill(CostSource.Components, "Drivetrain"));
        categories.Series.Select(s => s.Key).Should().Equal("Chain", "Cassette");
        categories.Series.Should().OnlyContain(s => s.Source == CostSource.Components);
    }

    [Fact]
    public void Timeline_folds_the_tail_into_other()
    {
        var report = Report();
        for (var i = 0; i < 10; i++)
        {
            report.Lines.Add(Line(CostSource.Components, 10m + i, 2025, group: "Drivetrain", category: $"Cat{i}", itemId: 100 + i));
        }

        var series = CostAggregator.Timeline(report, new CostFilter(), new CostDrill(CostSource.Components, "Drivetrain"), maxSeries: 4).Series;

        series.Should().HaveCount(4);
        series.Last().Key.Should().Be(CostAggregator.OtherSeries);
        series.Sum(s => s.Total).Should().Be(500m + Enumerable.Range(0, 10).Sum(i => 10m + i));
    }

    [Fact]
    public void Drill_passes_through_levels_with_a_single_slice()
    {
        var report = Report();
        var filter = new CostFilter();

        // Bikes → "Bikes" → "Bikes": straight to the bikes, and back up in one step.
        var bikes = CostAggregator.Drill(report, filter, new CostDrill(), "Bikes");
        bikes.Should().Be(new CostDrill(CostSource.Bikes, "Bikes", "Bikes"));
        CostAggregator.Breakdown(report, filter, bikes).Select(s => s.Label).Should().Equal("Item3");
        CostAggregator.Up(report, filter, bikes, 2).Should().Be(new CostDrill());

        // Components has two groups — a real level; Wheels holds only tyres, so it opens on the tyres.
        var components = CostAggregator.Drill(report, filter, new CostDrill(), "Components");
        components.Should().Be(new CostDrill(CostSource.Components));
        var wheels = CostAggregator.Drill(report, filter, components, "Wheels");
        wheels.Should().Be(new CostDrill(CostSource.Components, "Wheels", "Tire"));
        CostAggregator.Up(report, filter, wheels, 2).Should().Be(components);
    }

    [Fact]
    public void Timeline_drills_down_to_items()
    {
        var items = CostAggregator.Timeline(Report(), new CostFilter(), new CostDrill(CostSource.Components, "Drivetrain", "Chain"));

        items.Series.Should().ContainSingle().Which.Should().Match<CostTimelineSeries>(s => s.Key == "Components:10" && s.Label == "Item10");
    }

    [Fact]
    public void Colours_follow_the_entity_siblings_differ_and_the_largest_child_takes_its_parents_colour()
    {
        var colors = CostColors.Build(Report());

        colors.Of(new CostDrill(), "Bikes").Should().Be(CostColors.Sources[CostSource.Bikes]);
        var drivetrain = colors.Group(CostSource.Components, "Drivetrain");
        drivetrain.Should().Be(CostColors.Slots[1]);                               // the components' largest group: their own hue
        colors.Group(CostSource.Components, "Wheels").Should().NotBe(drivetrain);  // siblings differ
        colors.Category(CostSource.Components, "Drivetrain", "Chain").Should().Be(drivetrain);
        colors.Category(CostSource.Components, "Drivetrain", "Cassette").Should().NotBe(drivetrain);
        colors.Of(new CostDrill(CostSource.Components, "Drivetrain", "Chain"), "Components:10").Should().Be(drivetrain);
        colors.Group(CostSource.Services, "Unknown").Should().Be(CostColors.Rest);
    }

    [Fact]
    public void Per_bike_totals_include_a_no_bike_bucket()
    {
        var bikes = CostAggregator.Bikes(Report(), new CostFilter());

        bikes.Single(b => b.BikeId == 3).Net.Should().Be(3580m);
        bikes.Single(b => b.BikeId is null).Net.Should().Be(120m);
        bikes.Single(b => b.BikeId == 4).PerKm.Should().Be(0.1m);
    }

    [Fact]
    public void Per_1000_km_divides_category_spending_by_the_km_in_scope()
    {
        // 2025, all bikes: chain 300 / 4000 km = 75 per 1000 km; tyre 100 → 25; expenses / bikes are not parts.
        var per1000 = CostAggregator.Per1000Km(Report(), new CostFilter { FromYear = 2025, ToYear = 2025 });

        per1000.Select(p => (p.Category, p.Per1000Km)).Should().Equal(("Chain", 75m), ("Cassette", 50m), ("Tire", 25m));
        per1000.Should().OnlyContain(p => p.Parts == 1 && p.Services == 0);
    }

    [Fact]
    public void Per_1000_km_counts_each_part_once_and_follows_the_bike_filter_and_is_empty_without_km()
    {
        var report = Report();
        report.Lines.Add(Line(CostSource.Components, 50m, 2025, group: "Wheels", category: "Tire", itemId: 14));
        report.Lines.Add(Line(CostSource.Components, 50m, 2025, bikeId: 4, group: "Wheels", category: "Tire", itemId: 14));   // split over 2 bikes
        CostAggregator.Per1000Km(report, new CostFilter { FromYear = 2025, ToYear = 2025 }).Single(p => p.Category == "Tire").Parts.Should().Be(2);

        CostAggregator.Per1000Km(Report(), new CostFilter { FromYear = 2025, ToYear = 2025, BikeId = 4 })
            .Should().ContainSingle(p => p.Category == "Tire" && p.Per1000Km == 100m);   // 100 / 1000 km

        CostAggregator.Per1000Km(Report(), new CostFilter { BikeId = CostFilter.NoBike }).Should().BeEmpty();
    }

    [Fact]
    public void Ledger_groups_by_source_and_group_with_subtotals()
    {
        var ledger = CostAggregator.Ledger(Report(), new CostFilter());

        ledger.Select(g => (g.Source, g.Group, g.Total)).Should().Equal(
            (CostSource.Bikes, "Bikes", 3000m),
            (CostSource.Components, "Drivetrain", 500m),
            (CostSource.Components, "Wheels", 180m),
            (CostSource.Expenses, "Clothing", 120m));
    }
}
