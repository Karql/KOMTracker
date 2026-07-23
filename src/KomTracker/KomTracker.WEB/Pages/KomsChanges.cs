using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Segment;
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
    private string? _selectedActivityType;
    private CompassDirection? _selectedDirection;
    private LocationFilter? _locationFilter;
    private IEnumerable<ClubViewModel> _clubs = Enumerable.Empty<ClubViewModel>();
    private IEnumerable<EffortWithAthleteViewModel> _changes = Enumerable.Empty<EffortWithAthleteViewModel>();
    
    private EffortWithAthleteViewModel _change = default!;

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
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Koms changes", href: "koms-changes"),
        });

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
        if (!string.IsNullOrEmpty(_selectedActivityType)
            && !string.Equals(change.Effort.Segment.ActivityType, _selectedActivityType, StringComparison.OrdinalIgnoreCase)) return false;

        if (_selectedDirection.HasValue && change.Effort.Segment.Direction != _selectedDirection.Value) return false;

        if (_locationFilter is not null
            && GeoHelper.GetDistance(_locationFilter.Lat, _locationFilter.Lng, change.Effort.Segment.StartLatitude, change.Effort.Segment.StartLongitude) > _locationFilter.RadiusKm) return false;

        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return
            change.Effort.Segment.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true
            || change.Athlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);

    }

    private async Task OpenLocationDialogAsync()
    {
        var parameters = new DialogParameters<LocationFilterDialog>
        {
            { x => x.Initial, _locationFilter },
        };

        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };

        var dialog = await DialogService.ShowAsync<LocationFilterDialog>("Location", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is LocationFilter location)
        {
            _locationFilter = location;
            StateHasChanged();
        }
    }

    private void ClearLocationFilter() => _locationFilter = null;

    private Task<IEnumerable<ClubViewModel>> SearchClubs(string club)
    {
        return Task.FromResult(_clubs.Where(x => string.IsNullOrEmpty(club) || x.Name.Contains(club, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task SelectedClubChanged(ClubViewModel selectedClub)
    {
        await GetKomsCahngesAsync(selectedClub?.Id);
    }
}