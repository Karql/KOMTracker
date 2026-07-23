using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Application.Shared.Models.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class Koms
{
    [Parameter]
    [SupplyParameterFromQuery]
    public int? AthleteId { get; set; }

    private bool _loaded = false;
    private UserModel _user = default!;
    private string _searchString = "";
    private CompassDirection? _selectedDirection;
    private LocationFilter? _locationFilter;
    private IEnumerable<EffortViewModel> _koms = Enumerable.Empty<EffortViewModel>();
    private EffortViewModel _kom = default!;

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
            new BreadcrumbItem("Koms", href: "koms"),
        });

        _user = await UserService.GetCurrentUser();

        await GetAllKoms();

        _loaded = true;
    }

    private async Task GetAllKoms()
    {
        var athleteId = AthleteId ?? _user.AthleteId;

        _koms = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{athleteId}/koms")
            ?? Enumerable.Empty<EffortViewModel>();
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

    private bool Search(EffortViewModel kom)
    {
        if (_selectedDirection.HasValue && kom.Segment.Direction != _selectedDirection.Value) return false;
        if (_locationFilter is not null
            && GeoHelper.GetDistance(_locationFilter.Lat, _locationFilter.Lng, kom.Segment.StartLatitude, kom.Segment.StartLongitude) > _locationFilter.RadiusKm) return false;
        if (string.IsNullOrWhiteSpace(_searchString)) return true;
        if (kom.Segment.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }
        return false;
    }
}