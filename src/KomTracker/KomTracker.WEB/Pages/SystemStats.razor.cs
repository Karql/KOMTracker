using System.Globalization;
using System.Net.Http.Json;
using ApexCharts;
using KomTracker.Domain.Entities.SystemStats;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Color = MudBlazor.Color;

namespace KomTracker.WEB.Pages;

/// <summary>
/// Admin "System → Stats": one snapshot of the whole app (D-SYS-1) — counts, people, Bike Tracker usage and the health
/// of the background syncs. Read-only; refreshed by reloading.
/// </summary>
public partial class SystemStats
{
    [CascadingParameter] public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;

    private bool _loaded;
    private SystemStatsReport? _report;
    private ApexChartOptions<SystemMonth> _growthOptions = new();

    /// <summary>A refreshed thing and how old it may get before it looks stuck (amber).</summary>
    private sealed record Freshness(string Label, DateTime? At, TimeSpan Expected, string Note);

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem> { new("System", href: null, disabled: true), new("Stats", href: "system/stats") });

        _report = await Http.GetFromJsonAsync<SystemStatsReport>("admin/system-stats");
        _growthOptions = new ApexChartOptions<SystemMonth>
        {
            Chart = new Chart { Animations = new Animations { Enabled = true, Speed = 600 } },
            Stroke = new Stroke { Width = 3, Curve = Curve.Smooth },
            Fill = new Fill { Type = new List<FillType> { FillType.Gradient }, Gradient = new FillGradient { OpacityFrom = 0.35, OpacityTo = 0.02 } },
            Colors = new List<string> { "#fc4c02" },
            DataLabels = new DataLabels { Enabled = false },
            Grid = new Grid { BorderColor = "#eeeeee" },
            Yaxis = new List<YAxis> { new() { Min = 0, ForceNiceScale = true } }
        };
        _loaded = true;
    }

    private IEnumerable<Freshness> Freshnesses(SystemHealth health) => new[]
    {
        new Freshness("Activity sync (job)", health.LastJobSync, TimeSpan.FromHours(36), "nightly"),
        new Freshness("Segments refresh", health.LastSegmentRefresh, TimeSpan.FromHours(3), "hourly"),
        new Freshness("Athletes refresh", health.LastAthleteRefresh, TimeSpan.FromHours(36), "nightly"),
        new Freshness("Stats refresh", health.LastStatsRefresh, TimeSpan.FromHours(36), "nightly"),
        new Freshness("Clubs refresh", health.LastClubRefresh, TimeSpan.FromHours(24), "twice a day"),
        // Summaries are written only when something changed — old just means quiet, never "stuck".
        new Freshness("Last KOM change", health.LastKomChange, TimeSpan.MaxValue, "on change")
    };

    private bool IsStale(Freshness f)
        => f.At is not { } at || (f.Expected != TimeSpan.MaxValue && _report!.GeneratedAt - at > f.Expected);

    // ---- formatting ----------------------------------------------------------------------------------------------

    private static string N(int value) => Numbers.Format((decimal)value);

    private static string Recent(SystemCount count) => count.Recent > 0 ? $"+{N(count.Recent)} in {SystemStatsReport.RecentDays} days" : $"none new in {SystemStatsReport.RecentDays} days";

    /// <summary>"5 min ago", "3 h ago", "2 d ago" — relative to the snapshot.</summary>
    private string Ago(DateTime? utc)
    {
        if (utc is not { } at)
        {
            return "—";
        }

        var span = _report!.GeneratedAt - at;
        return span.TotalMinutes < 1 ? "just now"
            : span.TotalHours < 1 ? $"{(int)span.TotalMinutes} min ago"
            : span.TotalDays < 1 ? $"{(int)span.TotalHours} h ago"
            : span.TotalDays < 60 ? $"{(int)span.TotalDays} d ago"
            : $"{(int)(span.TotalDays / 30.4)} mo ago";
    }

    private static string Exact(DateTime? utc)
        => utc is { } at ? DateTime.SpecifyKind(at, DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "never";

    private static string ItemIcon(SystemItemType type) => type switch
    {
        SystemItemType.Bike => Icons.Material.Filled.PedalBike,
        SystemItemType.Component => Icons.Material.Filled.Build,
        SystemItemType.Installation => Icons.Material.Filled.Link,
        SystemItemType.Service => Icons.Material.Filled.Handyman,
        SystemItemType.Expense => Icons.Material.Filled.ReceiptLong,
        _ => Icons.Material.Filled.Warehouse
    };

    private static Color SyncColor(string status) => status switch
    {
        "Ok" => Color.Success,
        "NotFound" or "Deleted" => Color.Default,   // webhook for a deleted / private activity — normal
        "RateLimited" => Color.Warning,
        _ => Color.Error
    };
}
