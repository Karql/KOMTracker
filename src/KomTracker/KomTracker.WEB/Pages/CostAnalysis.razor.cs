using System.Globalization;
using System.Net.Http.Json;
using ApexCharts;
using KomTracker.API.Shared.CostAnalysis;
using KomTracker.Domain.Entities.CostAnalysis;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Currency;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ApexChartTooltip = ApexCharts.Tooltip;
using ApexChartLegend = ApexCharts.Legend;

namespace KomTracker.WEB.Pages;

/// <summary>
/// Cost analysis (Phase 4): the whole attributed ledger is loaded once and every card is a pure function of
/// (report, filter, drill) via <see cref="CostAggregator"/> — so filters, drill-down and chart/table switches are instant.
/// </summary>
public partial class CostAnalysis
{
    /// <summary>Pre-filter to one bike (the bike page's "Costs" link).</summary>
    [SupplyParameterFromQuery(Name = "bikeId")] public int? BikeIdQuery { get; set; }

    [CascadingParameter] public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ICurrencyPreference CurrencyPreference { get; set; } = default!;

    // Source palette — the same colour for a source in every chart and chip.
    internal static readonly IReadOnlyDictionary<CostSource, string> SourceColors = CostColors.Sources;

    private bool _loaded;
    private CostAnalysisReport _report = new();
    private string _currency = Currencies.Default;
    private IReadOnlyList<int> _years = Array.Empty<int>();
    private CostColors _colors = CostColors.Build(new CostAnalysisReport());

    // Filter state
    private string _period = "all";            // "all" | "<year>" | "range"
    private int? _fromYear;
    private int? _toYear;
    private int? _bikeId;
    private readonly HashSet<CostSource> _sources = new(Enum.GetValues<CostSource>());
    private bool _net = true;

    // Card modes (chart / table) and the two independent drill-down paths (breakdown, timeline)
    private bool _breakdownTable;
    private bool _timelineTable;
    private bool _bikesTable;
    private CostDrill _drill = new();
    private CostDrill _timelineDrill = new();

    // Derived for the current filter (recomputed on every change)
    private CostFilter _filter = new();
    private CostKpis _kpis = new(0, 0, 0, 0, null, null, null, null, 0);
    private IReadOnlyList<CostSlice> _slices = Array.Empty<CostSlice>();
    private CostTimeline _timeline = new(Array.Empty<string>(), Array.Empty<CostTimelineSeries>());
    private IReadOnlyList<CostBikeTotal> _bikeTotals = Array.Empty<CostBikeTotal>();
    private IReadOnlyList<CategoryTile> _categories = Array.Empty<CategoryTile>();
    private IReadOnlyList<CostPer1000Km> _per1000 = Array.Empty<CostPer1000Km>();
    private IReadOnlyList<CostLedgerGroup> _ledger = Array.Empty<CostLedgerGroup>();

    // Fresh chart instances (with their entry animation) only for the cards that actually changed: a drill-down
    // redraws its own card, a filter change redraws everything.
    private int _breakdownVersion;
    private int _timelineVersion;
    private int _cardsVersion;

    private ApexChartOptions<CostSlice> _donutOptions = new();
    private ApexChartOptions<TimelineCell> _timelineOptions = new();
    private ApexChartOptions<CostBikeTotal> _bikesOptions = new();
    private ApexChartOptions<CategoryTile> _treemapOptions = new();

    internal sealed record CategoryTile(string Label, decimal Amount, string Color);

    /// <summary>One bar segment of the timeline chart (a series' amount in a period).</summary>
    internal sealed record TimelineCell(string Period, decimal Value);

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem> { new("Costs", href: "costs") });

        _currency = await CurrencyPreference.GetAsync();
        _report = await Http.GetFromJsonAsync<CostAnalysisReport>("cost-analysis") ?? new CostAnalysisReport();
        _years = CostAggregator.Years(_report);
        _colors = CostColors.Build(_report);
        _bikeId = BikeIdQuery is int b && _report.Bikes.Any(x => x.Id == b) ? b : null;

        BuildChartOptions();
        Recompute();
        _loaded = true;
    }

    // ---- filter changes -------------------------------------------------------------------------------------------

    private void SetPeriod(string period)
    {
        _period = period;
        if (period == "all")
        {
            _fromYear = _toYear = null;
        }
        else if (int.TryParse(period, out var year))
        {
            _fromYear = _toYear = year;
        }
        else if (_fromYear is null && _years.Count > 0)
        {
            // Range: start with the last three years.
            _toYear = _years.First();
            _fromYear = _years.Where(y => y >= _toYear - 2).Min();
        }

        Recompute();
    }

    private void SetRange(int? from, int? to)
    {
        _fromYear = from;
        _toYear = to;
        Recompute();
    }

    private void SetBike(int? bikeId)
    {
        _bikeId = bikeId;
        Recompute();
    }

    private void ToggleSource(CostSource source)
    {
        if (!_sources.Remove(source))
        {
            _sources.Add(source);
        }

        if (_drill.Source == source)
        {
            _drill = new CostDrill();
        }

        if (_timelineDrill.Source == source)
        {
            _timelineDrill = new CostDrill();
        }

        Recompute();
    }

    private void SetNet(bool net)
    {
        _net = net;
        Recompute();
    }

    // ---- drill-down -----------------------------------------------------------------------------------------------

    // Both drills skip levels that split into a single slice (CostAggregator.Drill / Up): bikes go straight to the bikes.
    private void DrillInto(CostSlice slice)
    {
        if (_drill.Level < 3) // items are the last level (they link to the item instead)
        {
            _drill = CostAggregator.Drill(_report, _filter, _drill, slice.Key);
            RecomputeBreakdown();
        }
    }

    private void DrillTo(int level)
    {
        _drill = CostAggregator.Up(_report, _filter, _drill, level);
        RecomputeBreakdown();
    }

    // Timeline drill: sources → groups → categories → items.
    private void DrillTimeline(string key)
    {
        if (key == CostAggregator.OtherSeries || _timelineDrill.Level >= 3)
        {
            return;
        }

        _timelineDrill = CostAggregator.Drill(_report, _filter, _timelineDrill, key);
        RecomputeTimeline();
    }

    private void TimelineTo(int level)
    {
        _timelineDrill = CostAggregator.Up(_report, _filter, _timelineDrill, level);
        RecomputeTimeline();
    }

    private Task OnTimelineSelected(SelectedData<TimelineCell> data)
    {
        if (data.SeriesIndex >= 0 && data.SeriesIndex < _timeline.Series.Count)
        {
            DrillTimeline(_timeline.Series[data.SeriesIndex].Key);
        }

        return Task.CompletedTask;
    }

    // Breadcrumbs of a drill path. A level that was passed through (a single slice) is no stop: plain text, or left out
    // when it only repeats the next label (Bikes › Bikes › Bikes → Bikes).
    private IEnumerable<(string Label, int Level, bool Link)> Crumbs(CostDrill drill)
    {
        var path = new List<(string Label, int Level)> { ("All sources", 0) };
        if (drill.Source is { } source) path.Add((source.ToString(), 1));
        if (drill.Group is { } group) path.Add((group, 2));
        if (drill.Category is { } category) path.Add((category, 3));

        for (var i = 0; i < path.Count; i++)
        {
            var (label, level) = path[i];
            var here = level == drill.Level;
            var passedThrough = !here && CostAggregator.IsPassThrough(_report, _filter, drill.At(level));
            if (passedThrough && path[i + 1].Label == label)
            {
                continue;
            }

            yield return (label, level, !here && !passedThrough);
        }
    }

    private Task OnSliceSelected(SelectedData<CostSlice> data)
    {
        var slice = data.DataPoint?.Items?.FirstOrDefault();
        if (slice is not null && _drill.Level < 3)
        {
            DrillInto(slice);
        }

        return Task.CompletedTask;
    }

    // ---- recompute ------------------------------------------------------------------------------------------------

    private void Recompute()
    {
        _filter = new CostFilter
        {
            FromYear = _fromYear,
            ToYear = _toYear,
            BikeId = _bikeId,
            Sources = new HashSet<CostSource>(_sources),
            Net = _net
        };

        _kpis = CostAggregator.Kpis(_report, _filter, DateTime.Today);
        _bikeTotals = CostAggregator.Bikes(_report, _filter);
        // Each tile in its category's own colour — the same as in the donut and the year bars. Without the bike purchase:
        // over all time it takes most of the area and squeezes the parts into slivers (the donut shows that split).
        _categories = CostAggregator.Categories(_report, _filter)
            .Where(c => c.Source != CostSource.Bikes)
            .Take(30)
            .Select(c => new CategoryTile(c.Category, c.Amount, _colors.Category(c.Source, c.Group, c.Category)))
            .ToList();
        _per1000 = CostAggregator.Per1000Km(_report, _filter);
        _ledger = CostAggregator.Ledger(_report, _filter);
        _cardsVersion++;

        RecomputeBreakdown();
        RecomputeTimeline();
    }

    private void RecomputeBreakdown()
    {
        _slices = CostAggregator.Breakdown(_report, _filter, _drill);
        _breakdownVersion++;
    }

    private void RecomputeTimeline()
    {
        _timeline = CostAggregator.Timeline(_report, _filter, _timelineDrill);
        _timelineVersion++;

        // ApexCharts scales a stacked axis to each bar's net sum, so a sale (negative segment) clips the positive
        // stack above it — size the axis to the tallest positive and the deepest negative stack instead.
        var periods = Enumerable.Range(0, _timeline.Periods.Count).ToList();
        var top = (double)periods.Select(p => _timeline.Series.Sum(s => Math.Max(Math.Round(s.Values[p]), 0m))).DefaultIfEmpty().Max();
        var bottom = (double)periods.Select(p => _timeline.Series.Sum(s => Math.Min(Math.Round(s.Values[p]), 0m))).DefaultIfEmpty().Min();
        var step = NiceStep((top - bottom) / 5);
        var max = Math.Ceiling(top / step) * step;
        var min = Math.Floor(bottom / step) * step;
        var axis = _timelineOptions.Yaxis[0];
        axis.Max = max;
        axis.Min = min;
        axis.TickAmount = (int)Math.Round((max - min) / step);
    }

    // 1 / 2 / 2.5 / 5 × 10ⁿ at or above the given raw step — round axis ticks (5 000 zł, not 4 393 zł).
    private static double NiceStep(double raw)
    {
        if (raw <= 0)
        {
            return 1;
        }

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        return new[] { 1, 2, 2.5, 5, 10 }.Select(m => m * magnitude).First(s => s >= raw);
    }

    // ---- presentation helpers -------------------------------------------------------------------------------------

    private string Money(decimal amount) => Currencies.Format(Math.Round(amount), _currency);

    private string MoneyPrecise(decimal amount) => Currencies.Format(Math.Round(amount, 2), _currency);

    // Every entity keeps its own colour across cards and filters (CostColors); past the palette's 8 slots a view
    // greys out the tail rather than repeating colours.
    private string SliceColor(CostSlice slice, int index)
        => index >= CostColors.Slots.Count ? CostColors.Rest : _colors.Of(_drill, slice.Key);

    private string SliceColorOf(CostSlice slice) => SliceColor(slice, _slices.ToList().IndexOf(slice));

    private string TimelineColor(CostTimelineSeries series)
        => series.Key == CostAggregator.OtherSeries ? CostColors.Rest : _colors.Of(_timelineDrill, series.Key);

    private static IReadOnlyList<TimelineCell> Cells(CostTimeline timeline, CostTimelineSeries series)
        => timeline.Periods.Select((p, i) => new TimelineCell(p, Math.Round(series.Values[i]))).ToList();

    // "8 parts · 1 service" — what the category's spending is made of.
    private static string Bought(CostPer1000Km item)
        => string.Join(" · ", new[] { (Count: item.Parts, Noun: "part"), (Count: item.Services, Noun: "service") }
            .Where(x => x.Count > 0)
            .Select(x => $"{x.Count} {x.Noun}{(x.Count == 1 ? "" : "s")}"));

    private string PeriodLabel => _period switch
    {
        "all" => "All time",
        "range" => $"{_fromYear?.ToString() ?? "…"} – {_toYear?.ToString() ?? "now"}",
        _ => _period
    };

    private string BikeLabel => _bikeId switch
    {
        null => "All bikes",
        CostFilter.NoBike => CostAggregator.NoBikeName,
        int id => _report.Bikes.FirstOrDefault(b => b.Id == id)?.Name ?? "Bike"
    };

    private static string ItemHref(CostLine line) => line.Source switch
    {
        CostSource.Bikes => $"bikes/{line.ItemId}",
        CostSource.Components => $"components/{line.ItemId}",
        CostSource.Services => $"services/{line.ItemId}",
        _ => $"expenses/{line.ItemId}"
    };

    private string? BikeName(int? bikeId)
        => bikeId is int id ? _report.Bikes.FirstOrDefault(b => b.Id == id)?.Name : null;

    // ---- chart options --------------------------------------------------------------------------------------------

    // JS formatter for amounts in the chosen currency ("1 500 zł" / "€1 500") — charts format on the JS side.
    private string JsMoney()
    {
        var c = Currencies.Get(_currency);
        var symbol = c.Symbol.Replace("'", "\\'");
        return c.SymbolAfter
            ? $"function(v) {{ return Math.round(v).toLocaleString('pl-PL') + ' {symbol}'; }}"
            : $"function(v) {{ return '{symbol}' + Math.round(v).toLocaleString('pl-PL'); }}";
    }

    private void BuildChartOptions()
    {
        var money = JsMoney();

        _donutOptions = new ApexChartOptions<CostSlice>
        {
            DataLabels = new DataLabels { Enabled = false },
            Chart = new Chart { Animations = new Animations { Enabled = true, Speed = 600 } },
            Legend = new ApexChartLegend { Show = false },
            Stroke = new Stroke { Width = 2, Colors = new List<string> { "#fff" } },
            States = new States { Hover = new StatesHover { Filter = new StatesFilter { Type = StatesFilterType.darken } } },
            PlotOptions = new PlotOptions
            {
                Pie = new PlotOptionsPie
                {
                    ExpandOnClick = false,
                    Donut = new PlotOptionsDonut
                    {
                        Size = "70%",
                        Labels = new DonutLabels
                        {
                            Show = true,
                            Value = new DonutLabelValue { Formatter = money, FontSize = "22px", FontWeight = "800" },
                            Total = new DonutLabelTotal
                            {
                                Show = true,
                                ShowAlways = true,
                                Label = "Total",
                                Formatter = $"function(w) {{ var f = {money}; return f(w.globals.seriesTotals.reduce(function(a, b) {{ return a + b; }}, 0)); }}"
                            }
                        }
                    }
                }
            },
            Tooltip = new ApexChartTooltip { Y = new TooltipY { Formatter = money } }
        };

        _timelineOptions = new ApexChartOptions<TimelineCell>
        {
            DataLabels = new DataLabels { Enabled = false },
            Chart = new Chart { Stacked = true, Animations = new Animations { Enabled = true, Speed = 600 } },
            PlotOptions = new PlotOptions { Bar = new PlotOptionsBar { BorderRadius = 4, ColumnWidth = "55%" } },
            Legend = new ApexChartLegend { Position = LegendPosition.Top, HorizontalAlign = ApexCharts.Align.Left },
            Yaxis = new List<YAxis> { new() { Labels = new YAxisLabels { Formatter = money } } },
            Tooltip = new ApexChartTooltip { Shared = true, Intersect = false, Y = new TooltipY { Formatter = money } },
            Grid = new Grid { BorderColor = "#eeeeee" }
        };

        _bikesOptions = new ApexChartOptions<CostBikeTotal>
        {
            Chart = new Chart { Animations = new Animations { Enabled = true, Speed = 600 } },
            PlotOptions = new PlotOptions { Bar = new PlotOptionsBar { Horizontal = true, BorderRadius = 4, BarHeight = "62%" } },
            DataLabels = new DataLabels { Enabled = true, Formatter = money, Style = new DataLabelsStyle { FontSize = "11px", FontWeight = "700", Colors = new List<string> { "#ffffff" } } },
            Legend = new ApexChartLegend { Show = false },
            Xaxis = new XAxis { Labels = new XAxisLabels { Formatter = money } },
            Tooltip = new ApexChartTooltip { Y = new TooltipY { Formatter = money } },
            Grid = new Grid { BorderColor = "#eeeeee" }
        };

        _treemapOptions = new ApexChartOptions<CategoryTile>
        {
            Chart = new Chart { Animations = new Animations { Enabled = true, Speed = 600 } },
            Legend = new ApexChartLegend { Show = false },
            PlotOptions = new PlotOptions { Treemap = new PlotOptionsTreemap { Distributed = false, EnableShades = false } },
            DataLabels = new DataLabels { Enabled = true, Style = new DataLabelsStyle { FontSize = "13px", FontWeight = "700" } },
            Tooltip = new ApexChartTooltip { Y = new TooltipY { Formatter = money } }
        };
    }
}
