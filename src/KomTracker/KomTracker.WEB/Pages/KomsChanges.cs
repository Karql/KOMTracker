using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.WEB.Helpers;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class KomsChanges
{
    private bool _loaded = false;
    private UserModel _user = default!;
    private string _searchString = "";
    private IEnumerable<ClubViewModel> _clubs = Enumerable.Empty<ClubViewModel>();
    private IEnumerable<EffortWithAthleteViewModel> _changes = Enumerable.Empty<EffortWithAthleteViewModel>();
    
    private EffortWithAthleteViewModel _change;

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
            new BreadcrumbItem("Koms changes", href: "/koms-changes"),
        };

        _user = await UserService.GetCurrentUser();

        await Task.WhenAll(
            GetKomsCahngesAsync(),
            GetUserClubsAsync()
        );

        _loaded = true;
    }

    private async Task GetUserClubsAsync()
    {
        _clubs = await Http.GetFromJsonAsync<ClubViewModel[]>($"athletes/{_user.AthleteId}/clubs")
            ?? Enumerable.Empty<ClubViewModel>();
    }

    private async Task GetKomsCahngesAsync(long? clubId = null)
    {
        var query = clubId.HasValue ? $"?club_id={clubId.Value}" : String.Empty;

        _changes = await Http.GetFromJsonAsync<EffortWithAthleteViewModel[]>($"stats/koms-changes{query}")
            ?? Enumerable.Empty<EffortWithAthleteViewModel>();
    }

    private bool SearchChanges(EffortWithAthleteViewModel change)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return
            change.Effort.Segment.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true
            || change.Athlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);

    }

    private Task<IEnumerable<ClubViewModel>> SearchClubs(string club)
    {
        return Task.FromResult(_clubs.Where(x => string.IsNullOrEmpty(club) || x.Name.Contains(club, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task SelectedClubChanged(ClubViewModel selectedClub)
    {
        await GetKomsCahngesAsync(selectedClub?.Id);
    }
}