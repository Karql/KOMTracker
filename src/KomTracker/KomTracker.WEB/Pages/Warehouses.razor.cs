using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Warehouse;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Warehouses
{
    private const string ViewPreferenceKey = "warehouses";

    private bool _loaded;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private IEnumerable<WarehouseViewModel> _warehouses = Enumerable.Empty<WarehouseViewModel>();

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IPreferenceService Preferences { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Warehouses", href: "warehouses"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);

        await LoadWarehousesAsync();

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

    private IEnumerable<WarehouseViewModel> FilteredWarehouses => string.IsNullOrWhiteSpace(_searchString)
        ? _warehouses
        : _warehouses.Where(w => w.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true);

    private async Task LoadWarehousesAsync()
    {
        _warehouses = await Http.GetFromJsonAsync<WarehouseViewModel[]>("warehouses")
            ?? Enumerable.Empty<WarehouseViewModel>();
    }

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditWarehouseDialog>("Add warehouse", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadWarehousesAsync();
        }
    }

    private async Task EditAsync(WarehouseViewModel warehouse)
    {
        var parameters = new DialogParameters<AddEditWarehouseDialog> { { x => x.Warehouse, warehouse } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditWarehouseDialog>("Edit warehouse", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadWarehousesAsync();
        }
    }

    private async Task DeleteAsync(WarehouseViewModel warehouse)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete warehouse",
            $"Delete \"{warehouse.Name}\"? Components stored here will keep existing but lose their location.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"warehouses/{warehouse.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Warehouse deleted", Severity.Success);
            await LoadWarehousesAsync();
        }
        else
        {
            Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }
}
