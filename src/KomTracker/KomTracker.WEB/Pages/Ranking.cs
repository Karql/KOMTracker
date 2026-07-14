using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.Ranking;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Segment;
using KomTracker.WEB.Helpers;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class Ranking
{
    private enum RankingType
    {
        Total,
        KomsChanges
    };

    private readonly IEnumerable<(RankingType Value, string Label)> _rankingTypes = new[]
    {
        (RankingType.Total, "Total"),
        (RankingType.KomsChanges, "Koms changes"),
    };

    private bool _loaded = false;
    private UserModel _user = default!;
    private string _searchString = "";
    private IEnumerable<ClubViewModel> _clubs = Enumerable.Empty<ClubViewModel>();
    private IEnumerable<AthleteRankingViewModel> _ranking = Enumerable.Empty<AthleteRankingViewModel>();
    private readonly ExtendedCategoryEnum[] _extendedCategories = Enum.GetValues<ExtendedCategoryEnum>().OrderByDescending(x => x).ToArray();

    private AthleteRankingViewModel _item = default!;
    private RankingType _selectedRankingType = RankingType.Total;
    private string? _selectedActivityType = null;
    private ClubViewModel? _selectedClub = null;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Ranking", href: "/ranking"),
        };

        _user = await UserService.GetCurrentUser();

        await Task.WhenAll(
            GetRankingAsync(),
            GetUserClubsAsync()
        );

        _loaded = true;
    }

    private async Task GetUserClubsAsync()
    {
        _clubs = await Http.GetFromJsonAsync<ClubViewModel[]>($"athletes/{_user.AthleteId}/clubs")
            ?? Enumerable.Empty<ClubViewModel>();
    }

    private async Task GetRankingAsync()
    {
        var qParams = new Dictionary<string, string?>();

        if (_selectedClub?.Id != null)
        {
            qParams.Add("club_id", _selectedClub.Id.ToString());
        }

        if (!string.IsNullOrEmpty(_selectedActivityType))
        {
            qParams.Add("activity_type", _selectedActivityType);
        }

        _ranking = await Http.GetFromJsonAsync<AthleteRankingViewModel[]>(QueryHelpers.AddQueryString("ranking", qParams))
            ?? Enumerable.Empty<AthleteRankingViewModel>();
    }

    private bool SearchRanking(AthleteRankingViewModel item)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return item.Athlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SelectedActivityTypeChanged(string selectedActivityType)
    {
        _selectedActivityType = selectedActivityType;
        await GetRankingAsync();
    }

    private async Task SelectedClubChanged(ClubViewModel selectedClub)
    {
        _selectedClub = selectedClub;
        await GetRankingAsync();
    }

    // Ranking type = Total: reuse the athlete's koms endpoint and filter client-side by the
    // selected activity type (+ the clicked category, or all for the "Koms" total column).
    private async Task OpenTotalKomsAsync(AthleteRankingViewModel row, ExtendedCategoryEnum? category, string label)
    {
        var koms = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{row.Athlete.AthleteId}/koms")
            ?? Array.Empty<EffortViewModel>();

        IEnumerable<EffortViewModel> filtered = koms;

        if (!string.IsNullOrEmpty(_selectedActivityType))
        {
            filtered = filtered.Where(x => string.Equals(x.Segment.ActivityType, _selectedActivityType, StringComparison.OrdinalIgnoreCase));
        }

        if (category.HasValue)
        {
            filtered = filtered.Where(x => x.Segment.ExtendedCategory == category.Value);
        }

        await ShowKomsAsync(BuildTitle(row, label), filtered.ToArray());
    }

    // Ranking type = Koms changes: the new/lost lists live in the precomputed stats, fetched lazily.
    private async Task OpenKomsChangesAsync(AthleteRankingViewModel row, string period, string direction, string label, int count)
    {
        if (count <= 0) return;

        var qParams = new Dictionary<string, string?>
        {
            { "athlete_id", row.Athlete.AthleteId.ToString() },
            { "period", period },
            { "direction", direction },
        };

        if (!string.IsNullOrEmpty(_selectedActivityType))
        {
            qParams.Add("activity_type", _selectedActivityType);
        }

        var koms = await Http.GetFromJsonAsync<EffortViewModel[]>(
            QueryHelpers.AddQueryString("ranking/koms-changes-details", qParams))
            ?? Array.Empty<EffortViewModel>();

        await ShowKomsAsync(BuildTitle(row, label), koms);
    }

    private string BuildTitle(AthleteRankingViewModel row, string label)
    {
        var suffix = string.IsNullOrEmpty(_selectedActivityType)
            ? string.Empty
            : $" · {ActivityTypeHelper.GetActivityTypeName(_selectedActivityType)}";

        return $"{row.Athlete.FullName} — {label}{suffix}";
    }

    private async Task ShowKomsAsync(string title, IEnumerable<EffortViewModel> efforts)
    {
        var parameters = new DialogParameters<KomsListDialog>
        {
            { x => x.Efforts, efforts },
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraExtraLarge,
            FullWidth = true,
            CloseButton = true
        };

        await DialogService.ShowAsync<KomsListDialog>(title, parameters, options);
    }
}