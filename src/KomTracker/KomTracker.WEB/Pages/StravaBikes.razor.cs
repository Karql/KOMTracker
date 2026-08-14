using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class StravaBikes
{
    private const string ViewPreferenceKey = "strava-bikes";

    private bool _loaded;
    private bool _syncing;
    private bool _showRetired;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private StravaSyncStatusViewModel _status = new();
    private IEnumerable<StravaBikeViewModel> _bikes = Enumerable.Empty<StravaBikeViewModel>();

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IPreferenceService Preferences { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Strava bikes", href: "bike-tracker/strava-bikes"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);

        await LoadAsync();

        _loaded = true;
    }

    private async Task SetViewAsync(ListViewMode mode)
    {
        if (_view == mode)
        {
            return;
        }

        _view = mode;
        await Preferences.SetListViewAsync(ViewPreferenceKey, mode);
    }

    private async Task LoadAsync()
    {
        _status = await Http.GetFromJsonAsync<StravaSyncStatusViewModel>("bike-tracker/strava/sync-status")
            ?? new StravaSyncStatusViewModel();
        _bikes = await Http.GetFromJsonAsync<StravaBikeViewModel[]>("bike-tracker/strava/bikes")
            ?? Enumerable.Empty<StravaBikeViewModel>();
    }

    private async Task SyncAsync()
    {
        _syncing = true;

        try
        {
            var response = await Http.PostAsync("bike-tracker/strava/sync", null);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Synced from Strava", Severity.Success);
                await LoadAsync();
            }
            else
            {
                await response.ShowProblemAsync(Snackbar);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    private bool Search(StravaBikeViewModel bike)
    {
        if (!_showRetired && bike.Retired)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        return (bike.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (bike.Nickname?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (bike.BrandName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (bike.ModelName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true);
    }

    private IEnumerable<StravaBikeViewModel> FilteredBikes => _bikes.Where(Search);

    private static string DisplayName(StravaBikeViewModel bike)
        => !string.IsNullOrWhiteSpace(bike.Name) ? bike.Name!
            : !string.IsNullOrWhiteSpace(bike.Nickname) ? bike.Nickname!
            : bike.Id;

    private static string FormatKm(double km) => $"{km:N0} km";

    private void OpenBike(int id) => Navigation.NavigateTo($"bikes/{id}");

    private void ManageStravaAccess() => Navigation.NavigateTo("account?tab=strava");

    private async Task CreateAsync(StravaBikeViewModel bike)
    {
        // Note: no InitialDistanceKm seeding — mileage is derived from synced activities later (1d).
        var prefill = new SaveBikeViewModel
        {
            Name = DisplayName(bike),
            Brand = bike.BrandName,
            Model = bike.ModelName,
            Type = bike.SuggestedType,
            WeightKg = bike.WeightKg,
            StravaGearId = bike.Id
        };

        var parameters = new DialogParameters<AddEditBikeDialog> { { x => x.Prefill, prefill } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditBikeDialog>("Create bike from Strava", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task UnlinkAsync(StravaBikeViewModel bike)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Unlink bike",
            $"Unlink \"{DisplayName(bike)}\" from {(string.IsNullOrWhiteSpace(bike.LinkedBikeName) ? "its bike" : bike.LinkedBikeName)}?",
            yesText: "Unlink",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"bike-tracker/strava/bikes/{bike.Id}/link");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Bike unlinked", Severity.Success);
            await LoadAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }

    private async Task LinkAsync(StravaBikeViewModel bike)
    {
        var parameters = new DialogParameters<LinkStravaBikeDialog>
        {
            { x => x.GearId, bike.Id },
            { x => x.GearName, DisplayName(bike) }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<LinkStravaBikeDialog>("Link Strava bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }
}
