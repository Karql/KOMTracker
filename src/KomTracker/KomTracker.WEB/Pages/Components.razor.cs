using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Component;
using KomTracker.API.Shared.ViewModels.Warehouse;
using KomTracker.Domain.Entities.Component;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Components
{
    // localStorage key for this page's card/table preference.
    private const string ViewPreferenceKey = "components";

    private bool _loaded;
    private bool _includeInactive;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private ComponentCategoryGroup? _groupFilter;
    private int? _warehouseFilter;
    private IEnumerable<ComponentViewModel> _components = Enumerable.Empty<ComponentViewModel>();
    private IEnumerable<WarehouseViewModel> _warehouses = Enumerable.Empty<WarehouseViewModel>();

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
            new BreadcrumbItem("Components", href: "components"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);

        _warehouses = await Http.GetFromJsonAsync<WarehouseViewModel[]>("warehouses")
            ?? Enumerable.Empty<WarehouseViewModel>();

        await LoadComponentsAsync();

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

    private async Task LoadComponentsAsync()
    {
        _components = await Http.GetFromJsonAsync<ComponentViewModel[]>($"components?include_inactive={_includeInactive}")
            ?? Enumerable.Empty<ComponentViewModel>();
    }

    private async Task ShowArchivedAsync()
    {
        _includeInactive = true;
        await LoadComponentsAsync();
    }

    private bool Search(ComponentViewModel component)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        return (component.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (component.Brand?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || (component.Model?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || ComponentCategoryMetadata.DisplayName(component.Category).Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<ComponentViewModel> FilteredComponents => _components
        .Where(Search)
        .Where(c => _groupFilter is null || c.CategoryGroup == _groupFilter)
        .Where(c => _warehouseFilter is null || c.WarehouseId == _warehouseFilter);

    private void OpenDetails(int id) => Navigation.NavigateTo($"components/{id}");

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditComponentDialog>("Add component", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadComponentsAsync();
        }
    }

    private async Task EditAsync(ComponentViewModel component)
    {
        var parameters = new DialogParameters<AddEditComponentDialog> { { x => x.Component, component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditComponentDialog>("Edit component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadComponentsAsync();
        }
    }

    private async Task SellAsync(ComponentViewModel component)
    {
        var parameters = new DialogParameters<SellComponentDialog> { { x => x.Component, component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellComponentDialog>("Sell component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadComponentsAsync();
        }
    }

    private async Task ArchiveAsync(ComponentViewModel component)
    {
        var parameters = new DialogParameters<ArchiveComponentDialog> { { x => x.Component, component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<ArchiveComponentDialog>("Archive component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadComponentsAsync();
        }
    }

    private Task ActivateAsync(ComponentViewModel component) => ChangeLifecycleAsync(component, ComponentLifecycle.Active);

    private async Task ChangeLifecycleAsync(ComponentViewModel component, ComponentLifecycle lifecycle)
    {
        var body = new ChangeComponentLifecycleViewModel { Lifecycle = lifecycle };
        var response = await Http.PutAsJsonAsync($"components/{component.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add($"Component {lifecycle.ToString().ToLowerInvariant()}", Severity.Success);
            await LoadComponentsAsync();
        }
        else
        {
            Snackbar.Add($"Failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task DeleteAsync(ComponentViewModel component)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete component",
            $"Delete \"{component.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"components/{component.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Component deleted", Severity.Success);
            await LoadComponentsAsync();
        }
        else
        {
            Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private static Color LifecycleColor(ComponentLifecycle lifecycle) => lifecycle switch
    {
        ComponentLifecycle.Active => Color.Success,
        ComponentLifecycle.Archived => Color.Default,
        ComponentLifecycle.Sold => Color.Info,
        _ => Color.Default
    };
}
