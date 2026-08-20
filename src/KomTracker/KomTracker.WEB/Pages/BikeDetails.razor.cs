using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.Domain.Entities.Bike;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class BikeDetails
{
    [Parameter] public int Id { get; set; }

    private bool _loaded;
    private BikeViewModel? _bike;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Bikes", href: "bikes"),
            new BreadcrumbItem("Details", href: $"bikes/{Id}"),
        });

        await LoadAsync();

        _loaded = true;
    }

    private async Task LoadAsync()
    {
        var response = await Http.GetAsync($"bikes/{Id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _bike = null;
            return;
        }

        _bike = await response.Content.ReadFromJsonAsync<BikeViewModel>();
    }

    private async Task EditAsync()
    {
        if (_bike is null)
        {
            return;
        }

        var parameters = new DialogParameters<AddEditBikeDialog> { { x => x.Bike, _bike } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditBikeDialog>("Edit bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task SellAsync()
    {
        if (_bike is null)
        {
            return;
        }

        var parameters = new DialogParameters<SellBikeDialog> { { x => x.Bike, _bike } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellBikeDialog>("Sell bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task ArchiveAsync()
    {
        if (_bike is null)
        {
            return;
        }

        var parameters = new DialogParameters<ArchiveBikeDialog> { { x => x.Bike, _bike } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<ArchiveBikeDialog>("Archive bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private Task ActivateAsync() => ChangeLifecycleAsync(BikeLifecycle.Active);

    private async Task ChangeLifecycleAsync(BikeLifecycle lifecycle)
    {
        if (_bike is null)
        {
            return;
        }

        var body = new ChangeBikeLifecycleViewModel { Lifecycle = lifecycle };
        var response = await Http.PutAsJsonAsync($"bikes/{_bike.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add($"Bike {lifecycle.ToString().ToLowerInvariant()}", Severity.Success);
            await LoadAsync();
        }
        else
        {
            Snackbar.Add($"Failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task DeleteAsync()
    {
        if (_bike is null)
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete bike",
            $"Delete \"{_bike.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"bikes/{_bike.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Bike deleted", Severity.Success);
            Navigation.NavigateTo("bikes");
        }
        else
        {
            Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task UnlinkAsync()
    {
        if (_bike?.StravaGearId is null)
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Unlink from Strava",
            $"Unlink \"{_bike.Name}\" from its Strava bike?",
            yesText: "Unlink",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"bike-tracker/strava/bikes/{_bike.StravaGearId}/link");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Unlinked from Strava", Severity.Success);
            await LoadAsync();
        }
        else
        {
            Snackbar.Add($"Unlink failed ({(int)response.StatusCode}).", Severity.Error);
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
