using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Athlete;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.KomTakeover;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class BattleField
{
    private bool _loaded = false;
    private UserModel _user = default!;
    private string _searchString = "";

    private IEnumerable<ClubViewModel> _clubs = Enumerable.Empty<ClubViewModel>();
    private IEnumerable<KomTakeoverPairViewModel> _pairs = Enumerable.Empty<KomTakeoverPairViewModel>();

    private IReadOnlyList<PeriodOption> _periods = Array.Empty<PeriodOption>();
    private PeriodOption _selectedPeriod = default!;
    private string? _selectedActivityType = null;
    private ClubViewModel? _selectedClub = null;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Battle Field", href: "/battle-field"),
        };

        _periods = BuildPeriods();
        _selectedPeriod = _periods.First();

        _user = await UserService.GetCurrentUser();

        await Task.WhenAll(
            GetPairsAsync(),
            GetUserClubsAsync()
        );

        _loaded = true;
    }

    private IReadOnlyList<PeriodOption> BuildPeriods()
    {
        var now = DateTime.UtcNow;
        var startYear = int.TryParse(Configuration["StartYear"], out var sy) ? sy : now.Year;

        var options = new List<PeriodOption>
        {
            new() { Label = "Last 30 days", DateFrom = now.AddDays(-30), DateTo = now },
            new() { Label = "Last 60 days", DateFrom = now.AddDays(-60), DateTo = now },
            new() { Label = "Last 90 days", DateFrom = now.AddDays(-90), DateTo = now },
            new() { Label = "Total", DateFrom = null, DateTo = null },
            new() { Label = "─────────", IsSeparator = true },
        };

        for (var year = now.Year; year >= startYear; year--)
        {
            options.Add(new PeriodOption
            {
                Label = year.ToString(),
                DateFrom = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateTo = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc)
            });
        }

        return options;
    }

    private async Task GetUserClubsAsync()
    {
        _clubs = await Http.GetFromJsonAsync<ClubViewModel[]>($"athletes/{_user.AthleteId}/clubs")
            ?? Enumerable.Empty<ClubViewModel>();
    }

    private async Task GetPairsAsync()
    {
        var qParams = new Dictionary<string, string?>();

        if (_selectedPeriod?.DateFrom != null)
        {
            qParams.Add("date_from", _selectedPeriod.DateFrom.Value.ToUniversalTime().ToString("o"));
        }

        if (_selectedPeriod?.DateTo != null)
        {
            qParams.Add("date_to", _selectedPeriod.DateTo.Value.ToUniversalTime().ToString("o"));
        }

        if (!string.IsNullOrEmpty(_selectedActivityType))
        {
            qParams.Add("activity_type", _selectedActivityType);
        }

        if (_selectedClub?.Id != null)
        {
            qParams.Add("club_id", _selectedClub.Id.ToString());
        }

        _pairs = await Http.GetFromJsonAsync<KomTakeoverPairViewModel[]>(
            QueryHelpers.AddQueryString("kom-takeovers/pairs", qParams))
            ?? Enumerable.Empty<KomTakeoverPairViewModel>();
    }

    private bool SearchPairs(KomTakeoverPairViewModel item)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return item.WinnerAthlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || item.LoserAthlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SelectedPeriodChanged(PeriodOption period)
    {
        if (period == null || period.IsSeparator) return;

        _selectedPeriod = period;
        await GetPairsAsync();
    }

    private async Task SelectedActivityTypeChanged(string selectedActivityType)
    {
        _selectedActivityType = selectedActivityType;
        await GetPairsAsync();
    }

    private async Task SelectedClubChanged(ClubViewModel selectedClub)
    {
        _selectedClub = selectedClub;
        await GetPairsAsync();
    }

    private Task OpenWinnerEffortsAsync(KomTakeoverPairViewModel pair)
        => OpenEffortsAsync(pair.WinnerAthlete, pair.LoserAthlete, pair.WinnerKoms);

    private Task OpenLoserEffortsAsync(KomTakeoverPairViewModel pair)
        => OpenEffortsAsync(pair.LoserAthlete, pair.WinnerAthlete, pair.LoserKoms);

    private async Task OpenEffortsAsync(AthleteViewModel taker, AthleteViewModel loser, int count)
    {
        if (count <= 0) return;

        var title = $"{taker.FullName} → {loser.FullName} ({count})";

        var parameters = new DialogParameters<BattleEffortsDialog>
        {
            { x => x.WinnerAthleteId, taker.AthleteId },
            { x => x.LoserAthleteId, loser.AthleteId },
            { x => x.DateFrom, _selectedPeriod?.DateFrom },
            { x => x.DateTo, _selectedPeriod?.DateTo },
            { x => x.ActivityType, _selectedActivityType },
            { x => x.Title, title },
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraExtraLarge,
            FullWidth = true,
            CloseButton = true
        };

        await DialogService.ShowAsync<BattleEffortsDialog>(title, parameters, options);
    }
}

public class PeriodOption
{
    public string Label { get; init; } = "";
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public bool IsSeparator { get; init; }
}
