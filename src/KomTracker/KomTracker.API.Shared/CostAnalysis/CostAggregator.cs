#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.CostAnalysis;

namespace KomTracker.API.Shared.CostAnalysis;

/// <summary>What the analysis is looking at — period, bike scope, sources and net / gross (D-COST-4/5/6).</summary>
public sealed record CostFilter
{
    /// <summary>First year included; null = from the beginning ("All time" when both are null).</summary>
    public int? FromYear { get; init; }

    /// <summary>Last year included; null = up to now.</summary>
    public int? ToYear { get; init; }

    /// <summary>Null = all bikes (incl. "General" + "Not on a bike"); a bike id; or <see cref="NoBike"/>.</summary>
    public int? BikeId { get; init; }

    public IReadOnlySet<CostSource> Sources { get; init; } = new HashSet<CostSource>(Enum.GetValues<CostSource>());

    /// <summary>True = resales are subtracted (default); false = spending only.</summary>
    public bool Net { get; init; } = true;

    /// <summary>Pseudo bike id: lines with no bike (expenses + parts never on a bike).</summary>
    public const int NoBike = -1;

    public bool IsAllTime => FromYear is null && ToYear is null;
}

/// <summary>A slice of a breakdown / bar: key (the drill-down value), label, amount and share of the visible total.</summary>
public sealed record CostSlice(string Key, string Label, decimal Amount, decimal Percent, int Lines);

/// <summary>Headline figures for the current filter.</summary>
public sealed record CostKpis(
    decimal Net, decimal Spent, decimal Recovered, decimal Km, decimal? PerKm, decimal? Per1000Km,
    decimal? PerYear, decimal? PerMonth, int Years);

/// <summary>
/// The timeline at a drill level: one series per source / group / category (the same keys as the breakdown), each
/// with an amount per period (years, or months within a single year).
/// </summary>
public sealed record CostTimeline(IReadOnlyList<string> Periods, IReadOnlyList<CostTimelineSeries> Series)
{
    public decimal Total(int period) => Series.Sum(s => s.Values[period]);
}

/// <summary>One stacked series of the timeline. <see cref="Source"/> = the source it belongs to (for its colour).</summary>
public sealed record CostTimelineSeries(string Key, string Label, CostSource Source, IReadOnlyList<decimal> Values)
{
    public decimal Total => Values.Sum();
}

/// <summary>Per-bike totals (all-bikes view). <see cref="BikeId"/> null = "no bike".</summary>
public sealed record CostBikeTotal(int? BikeId, string Name, decimal Net, decimal Km, decimal? PerKm, bool Inactive);

/// <summary>
/// Running cost of one component category in the current selection: what was spent on it ÷ the km ridden there,
/// per 1000 km (D-COST-8).
/// </summary>
public sealed record CostPer1000Km(string Category, ComponentCategory? Icon, decimal Amount, decimal Per1000Km, int Parts, int Services);

/// <summary>A ledger section (source → group) with its lines and subtotal.</summary>
public sealed record CostLedgerGroup(CostSource Source, string Group, decimal Total, IReadOnlyList<CostLine> Lines);

/// <summary>The drill-down path: Sources → Group → Category → items.</summary>
public sealed record CostDrill(CostSource? Source = null, string? Group = null, string? Category = null)
{
    public int Level => Source is null ? 0 : Group is null ? 1 : Category is null ? 2 : 3;

    /// <summary>This path cut back to <paramref name="level"/>.</summary>
    public CostDrill At(int level) => level switch
    {
        <= 0 => new CostDrill(),
        1 => new CostDrill(Source),
        2 => new CostDrill(Source, Group),
        _ => this
    };

    /// <summary>One level deeper, into the slice / series with this key (items are the last level).</summary>
    public CostDrill Into(string key) => Level switch
    {
        0 => new CostDrill(Enum.Parse<CostSource>(key)),
        1 => this with { Group = key },
        2 => this with { Category = key },
        _ => this
    };
}

/// <summary>
/// Pure, client-side aggregation of a <see cref="CostAnalysisReport"/> (D-COST-1). Every view of the cost page is a
/// function of (report, filter[, drill]) — so filters and drill-down are instant and the maths is unit-tested here.
/// </summary>
public static class CostAggregator
{
    public const string NoBikeName = "No bike";

    /// <summary>Lines matching the filter (net: costs + resales; gross: costs only).</summary>
    public static IEnumerable<CostLine> Filter(CostAnalysisReport report, CostFilter filter)
        => report.Lines.Where(l => Matches(l, filter) && (filter.Net || l.Kind == CostKind.Cost));

    private static bool Matches(CostLine line, CostFilter filter)
    {
        if (!filter.Sources.Contains(line.Source))
        {
            return false;
        }

        if (filter.BikeId == CostFilter.NoBike ? line.BikeId is not null : filter.BikeId is int bikeId && line.BikeId != bikeId)
        {
            return false;
        }

        if (filter.IsAllTime)
        {
            return true;
        }

        if (line.Date is null)
        {
            return false;
        }

        var year = line.Date.Value.Year;
        return (filter.FromYear is null || year >= filter.FromYear) && (filter.ToYear is null || year <= filter.ToYear);
    }

    /// <summary>Years with any cost / ride data, newest first — for the period chips.</summary>
    public static IReadOnlyList<int> Years(CostAnalysisReport report)
        => report.Lines.Where(l => l.Date is not null).Select(l => l.Date!.Value.Year)
            .Concat(report.Bikes.SelectMany(b => b.KmByYear.Keys))
            .Distinct()
            .OrderByDescending(y => y)
            .ToList();

    public static CostKpis Kpis(CostAnalysisReport report, CostFilter filter, DateTime today)
    {
        var lines = report.Lines.Where(l => Matches(l, filter)).ToList();
        var spent = lines.Where(l => l.Kind == CostKind.Cost).Sum(l => l.Amount);
        var recovered = -lines.Where(l => l.Kind == CostKind.Resale).Sum(l => l.Amount);
        var net = filter.Net ? spent - recovered : spent;

        var km = Km(report, filter);
        var years = YearSpan(report, filter, today);

        return new CostKpis(
            net, spent, recovered, km,
            km > 0 ? net / km : null,
            km > 0 ? net / km * 1000m : null,
            years > 0 ? net / years : null,
            years > 0 ? net / (years * 12m) : null,
            years);
    }

    /// <summary>Ride km of the bikes in scope over the period; seed km only in "All time" (D-COST-6).</summary>
    public static decimal Km(CostAnalysisReport report, CostFilter filter)
    {
        if (filter.BikeId == CostFilter.NoBike)
        {
            return 0m;
        }

        return report.Bikes
            .Where(b => filter.BikeId is null || b.Id == filter.BikeId)
            .Sum(b => BikeKm(b, filter));
    }

    private static decimal BikeKm(CostBike bike, CostFilter filter)
        => bike.KmByYear
               .Where(kv => (filter.FromYear is null || kv.Key >= filter.FromYear) && (filter.ToYear is null || kv.Key <= filter.ToYear))
               .Sum(kv => kv.Value)
           + (filter.IsAllTime ? bike.SeedKm : 0m);

    // Number of calendar years the period covers. An open end is filled from the data in scope (bike + sources): from
    // its first dated cost / ride year to this year — or, for a single retired bike, to its last cost / ride year.
    private static int YearSpan(CostAnalysisReport report, CostFilter filter, DateTime today)
    {
        var scope = filter with { FromYear = null, ToYear = null };
        var years = report.Lines.Where(l => l.Date is not null && Matches(l, scope)).Select(l => l.Date!.Value.Year)
            .Concat(report.Bikes.Where(b => filter.BikeId is null || b.Id == filter.BikeId).SelectMany(b => b.KmByYear.Keys))
            .ToList();

        var first = filter.FromYear ?? (years.Count > 0 ? years.Min() : (int?)null);
        if (first is null)
        {
            return 0;
        }

        var retiredBike = filter.BikeId is int id && report.Bikes.FirstOrDefault(b => b.Id == id) is { } bike
            && bike.Lifecycle != Domain.Entities.Bike.BikeLifecycle.Active;
        var last = filter.ToYear ?? (retiredBike && years.Count > 0 ? years.Max() : today.Year);

        return Math.Max(1, last - first.Value + 1);
    }

    /// <summary>The breakdown at the drill level: sources → groups → categories → items, largest first.</summary>
    public static IReadOnlyList<CostSlice> Breakdown(CostAnalysisReport report, CostFilter filter, CostDrill drill)
    {
        var lines = InDrill(Filter(report, filter), drill);

        IEnumerable<IGrouping<string, CostLine>> groups = drill.Level switch
        {
            0 => lines.GroupBy(l => l.Source.ToString()),
            1 => lines.GroupBy(l => l.Group),
            2 => lines.GroupBy(l => l.Category),
            _ => lines.GroupBy(ItemKey)
        };

        return ToSlices(groups.Select(g => (g.Key, Label: drill.Level == 3 ? g.First().ItemName : g.Key, Amount: g.Sum(l => l.Amount), Count: g.Count())));
    }

    /// <summary>The key of an item (the last drill level) — a slice / series key and the item colour's key.</summary>
    public static string ItemKey(CostLine line) => $"{line.Source}:{line.ItemId}";

    /// <summary>
    /// A level that splits into a single slice (bikes → "Bikes" → "Bikes", services → "Services"): showing it would be a
    /// pointless 100 % ring, so <see cref="Drill"/> and <see cref="Up"/> pass through it. The sources level never is.
    /// </summary>
    public static bool IsPassThrough(CostAnalysisReport report, CostFilter filter, CostDrill drill)
        => drill.Level is 1 or 2 && Breakdown(report, filter, drill).Count == 1;

    /// <summary>One level deeper into <paramref name="key"/>, then on through any single-slice levels.</summary>
    public static CostDrill Drill(CostAnalysisReport report, CostFilter filter, CostDrill drill, string key)
    {
        var next = drill.Into(key);
        while (IsPassThrough(report, filter, next))
        {
            next = next.Into(Breakdown(report, filter, next)[0].Key);
        }

        return next;
    }

    /// <summary>Back to <paramref name="level"/> — or further up, past single-slice levels.</summary>
    public static CostDrill Up(CostAnalysisReport report, CostFilter filter, CostDrill drill, int level)
    {
        var up = drill.At(level);
        while (IsPassThrough(report, filter, up))
        {
            up = up.At(up.Level - 1);
        }

        return up;
    }

    private static IReadOnlyList<CostSlice> ToSlices(IEnumerable<(string Key, string Label, decimal Amount, int Count)> items)
    {
        var list = items.OrderByDescending(i => i.Amount).ToList();
        var positive = list.Where(i => i.Amount > 0).Sum(i => i.Amount);
        return list.Select(i => new CostSlice(i.Key, i.Label, i.Amount, positive > 0 && i.Amount > 0 ? i.Amount / positive * 100m : 0m, i.Count)).ToList();
    }

    /// <summary>The label used for the series that gathers everything beyond the top ones.</summary>
    public const string OtherSeries = "Other";

    /// <summary>
    /// Per year (or per month within a single selected year), stacked by what the drill level splits into: sources →
    /// groups of a source → categories of a group → items (the timeline's own drill, independent of the breakdown). At most
    /// <paramref name="maxSeries"/> series — the largest; the rest are summed into "Other".
    /// </summary>
    public static CostTimeline Timeline(CostAnalysisReport report, CostFilter filter, CostDrill drill, int maxSeries = 8)
    {
        var lines = InDrill(Filter(report, filter), drill).Where(l => l.Date is not null).ToList();
        var singleYear = filter.FromYear is not null && filter.FromYear == filter.ToYear;

        if (lines.Count == 0 && !singleYear)
        {
            return new CostTimeline(Array.Empty<string>(), Array.Empty<CostTimelineSeries>());
        }

        // Period index of a line + the period labels.
        Func<CostLine, int> periodOf;
        IReadOnlyList<string> periods;
        if (singleYear)
        {
            periods = Enumerable.Range(1, 12).Select(m => new DateTime(filter.FromYear!.Value, m, 1).ToString("MMM")).ToList();
            periodOf = l => l.Date!.Value.Month - 1;
        }
        else
        {
            var from = filter.FromYear ?? lines.Min(l => l.Date!.Value.Year);
            var to = filter.ToYear ?? lines.Max(l => l.Date!.Value.Year);
            periods = Enumerable.Range(from, to - from + 1).Select(y => y.ToString()).ToList();
            periodOf = l => l.Date!.Value.Year - from;
        }

        Func<CostLine, string> keyOf = drill.Level switch
        {
            0 => l => l.Source.ToString(),
            1 => l => l.Group,
            2 => l => l.Category,
            _ => ItemKey
        };

        var series = lines
            .GroupBy(keyOf)
            .Select(g =>
            {
                var values = new decimal[periods.Count];
                foreach (var line in g)
                {
                    var p = periodOf(line);
                    if (p >= 0 && p < values.Length)
                    {
                        values[p] += line.Amount;
                    }
                }

                return new CostTimelineSeries(g.Key, drill.Level == 3 ? g.First().ItemName : g.Key, g.First().Source, values);
            })
            .OrderByDescending(s => s.Total)
            .ToList();

        // Level 0 always shows every source (stable colours / legend); deeper levels fold the tail into "Other".
        if (drill.Level > 0 && series.Count > maxSeries)
        {
            var tail = series.Skip(maxSeries - 1).ToList();
            var other = new decimal[periods.Count];
            foreach (var s in tail)
            {
                for (var i = 0; i < other.Length; i++)
                {
                    other[i] += s.Values[i];
                }
            }

            series = series.Take(maxSeries - 1).Append(new CostTimelineSeries(OtherSeries, OtherSeries, tail[0].Source, other)).ToList();
        }

        return new CostTimeline(periods, series);
    }

    private static IEnumerable<CostLine> InDrill(IEnumerable<CostLine> lines, CostDrill drill)
        => lines
            .Where(l => drill.Source is null || l.Source == drill.Source)
            .Where(l => drill.Group is null || l.Group == drill.Group)
            .Where(l => drill.Category is null || l.Category == drill.Category);

    /// <summary>Net cost and cost/km per bike for the period (ignores the bike filter), plus the "no bike" bucket.</summary>
    public static IReadOnlyList<CostBikeTotal> Bikes(CostAnalysisReport report, CostFilter filter)
    {
        var all = filter with { BikeId = null };
        var lines = Filter(report, all).ToList();

        var totals = report.Bikes.Select(b =>
        {
            var net = lines.Where(l => l.BikeId == b.Id).Sum(l => l.Amount);
            var km = BikeKm(b, all);
            return new CostBikeTotal(b.Id, b.Name, net, km, km > 0 ? net / km : null, b.Lifecycle != Domain.Entities.Bike.BikeLifecycle.Active);
        }).ToList();

        var noBike = lines.Where(l => l.BikeId is null).Sum(l => l.Amount);
        if (noBike != 0)
        {
            totals.Add(new CostBikeTotal(null, NoBikeName, noBike, 0m, null, false));
        }

        return totals.Where(t => t.Net != 0 || t.Km > 0).OrderByDescending(t => t.Net).ToList();
    }

    /// <summary>Category totals (source + group + category) for the treemap, largest first; positive amounts only.</summary>
    public static IReadOnlyList<(CostSource Source, string Group, string Category, decimal Amount)> Categories(CostAnalysisReport report, CostFilter filter)
        => Filter(report, filter)
            .GroupBy(l => (l.Source, l.Group, l.Category))
            .Select(g => (g.Key.Source, g.Key.Group, g.Key.Category, Amount: g.Sum(l => l.Amount)))
            .Where(x => x.Amount > 0)
            .OrderByDescending(x => x.Amount)
            .ToList();

    /// <summary>
    /// Parts' running cost per category in the current selection: component (+ service) spending on the category ÷ the
    /// km ridden by the bikes in scope over the period, per 1000 km — the same maths as the headline "per 1000 km", split
    /// by category, so it follows every filter. Empty when there are no km in scope. Most expensive first.
    /// </summary>
    public static IReadOnlyList<CostPer1000Km> Per1000Km(CostAnalysisReport report, CostFilter filter, int top = 8)
    {
        var km = Km(report, filter);
        if (km <= 0)
        {
            return Array.Empty<CostPer1000Km>();
        }

        return Filter(report, filter)
            .Where(l => l.Source is CostSource.Components or CostSource.Services && l.ComponentCategory is not null)
            .GroupBy(l => l.Category)
            .Select(g => (Category: g.Key, Icon: g.First().ComponentCategory, Amount: g.Sum(l => l.Amount),
                Parts: Bought(g, CostSource.Components), Services: Bought(g, CostSource.Services)))
            .Where(x => x.Amount > 0)
            .Select(x => new CostPer1000Km(x.Category, x.Icon, x.Amount, x.Amount / km * 1000m, x.Parts, x.Services))
            .OrderByDescending(x => x.Per1000Km)
            .Take(top)
            .ToList();
    }

    // How many distinct parts / services were bought in the selection (a part split over two bikes is still one part).
    private static int Bought(IEnumerable<CostLine> lines, CostSource source)
        => lines.Where(l => l.Source == source && l.Kind == CostKind.Cost).Select(l => l.ItemId).Distinct().Count();

    /// <summary>The ledger: lines grouped by source → group (largest group first), newest line first.</summary>
    public static IReadOnlyList<CostLedgerGroup> Ledger(CostAnalysisReport report, CostFilter filter)
        => Filter(report, filter)
            .GroupBy(l => (l.Source, l.Group))
            .Select(g => new CostLedgerGroup(g.Key.Source, g.Key.Group, g.Sum(l => l.Amount),
                g.OrderByDescending(l => l.Date ?? DateTime.MinValue).ThenBy(l => l.ItemName).ToList()))
            .OrderBy(g => g.Source)
            .ThenByDescending(g => g.Total)
            .ToList();
}
