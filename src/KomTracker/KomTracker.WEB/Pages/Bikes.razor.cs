using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.Domain.Entities.Bike;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Bikes
{
    // localStorage key for this page's card/table preference (reusable pattern per page).
    private const string ViewPreferenceKey = "bikes";

    private bool _loaded;
    private bool _includeInactive;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private IEnumerable<BikeViewModel> _bikes = Enumerable.Empty<BikeViewModel>();

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
            new BreadcrumbItem("Bikes", href: "bikes"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);

        await LoadBikesAsync();

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

    private async Task LoadBikesAsync()
    {
        _bikes = await Http.GetFromJsonAsync<BikeViewModel[]>($"bikes?include_inactive={_includeInactive}")
            ?? Enumerable.Empty<BikeViewModel>();
    }

    private bool Search(BikeViewModel bike)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        return (bike.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (bike.Brand?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (bike.Model?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true);
    }

    private IEnumerable<BikeViewModel> FilteredBikes => _bikes.Where(Search);

    private void OpenDetails(int id) => Navigation.NavigateTo($"bikes/{id}");

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditBikeDialog>("Add bike", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadBikesAsync();
        }
    }

    private async Task EditAsync(BikeViewModel bike)
    {
        var parameters = new DialogParameters<AddEditBikeDialog> { { x => x.Bike, bike } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditBikeDialog>("Edit bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadBikesAsync();
        }
    }

    private async Task SellAsync(BikeViewModel bike)
    {
        var parameters = new DialogParameters<SellBikeDialog> { { x => x.Bike, bike } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellBikeDialog>("Sell bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadBikesAsync();
        }
    }

    private Task ArchiveAsync(BikeViewModel bike) => ChangeLifecycleAsync(bike, BikeLifecycle.Archived);

    private Task ActivateAsync(BikeViewModel bike) => ChangeLifecycleAsync(bike, BikeLifecycle.Active);

    private async Task ChangeLifecycleAsync(BikeViewModel bike, BikeLifecycle lifecycle)
    {
        var body = new ChangeBikeLifecycleViewModel { Lifecycle = lifecycle };
        var response = await Http.PutAsJsonAsync($"bikes/{bike.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add($"Bike {lifecycle.ToString().ToLowerInvariant()}", Severity.Success);
            await LoadBikesAsync();
        }
        else
        {
            Snackbar.Add($"Failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task DeleteAsync(BikeViewModel bike)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete bike",
            $"Delete \"{bike.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"bikes/{bike.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Bike deleted", Severity.Success);
            await LoadBikesAsync();
        }
        else
        {
            Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private static Color LifecycleColor(BikeLifecycle lifecycle) => lifecycle switch
    {
        BikeLifecycle.Active => Color.Success,
        BikeLifecycle.Archived => Color.Default,
        BikeLifecycle.Sold => Color.Info,
        _ => Color.Default
    };
}
