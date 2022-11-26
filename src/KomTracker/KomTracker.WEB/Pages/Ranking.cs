using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.Ranking;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Shared.Models.Segment;
using KomTracker.WEB.Helpers;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

    private AthleteRankingViewModel _item;
    private RankingType _rankingType = RankingType.Total;

    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

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

    private async Task GetRankingAsync(long? clubId = null)
    {
        var query = clubId.HasValue ? $"?club_id={clubId.Value}" : String.Empty;

        _ranking = await Http.GetFromJsonAsync<AthleteRankingViewModel[]>($"ranking{query}")
            ?? Enumerable.Empty<AthleteRankingViewModel>();
    }

    private bool SearchRanking(AthleteRankingViewModel item)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return item.Athlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);

    }

    private Task<IEnumerable<ClubViewModel>> SearchClubs(string club)
    {
        return Task.FromResult(_clubs.Where(x => string.IsNullOrEmpty(club) || x.Name.Contains(club, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task SelectedClubChanged(ClubViewModel selectedClub)
    {
        await GetRankingAsync(selectedClub?.Id);
    }
}