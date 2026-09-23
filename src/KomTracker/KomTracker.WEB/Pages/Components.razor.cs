using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.API.Shared.ViewModels.Component;
using KomTracker.API.Shared.ViewModels.Warehouse;
using KomTracker.Domain.Entities.Component;
using KomTracker.WEB.Infrastructure;
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

    /// <summary>Install-state filter options.</summary>
    private enum InstallState { Installed, NotInstalled }

    private bool _loaded;
    private bool _includeInactive;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private ComponentCategoryGroup? _groupFilter;
    private int? _warehouseFilter;
    private InstallState? _installFilter;
    private int? _bikeFilter;
    private bool? _metaFilter;
    private IEnumerable<ComponentViewModel> _components = Enumerable.Empty<ComponentViewModel>();
    private IEnumerable<WarehouseViewModel> _warehouses = Enumerable.Empty<WarehouseViewModel>();
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
            new BreadcrumbItem("Components", href: "components"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);

        _warehouses = await Http.GetFromJsonAsync<WarehouseViewModel[]>("warehouses")
            ?? Enumerable.Empty<WarehouseViewModel>();
        _bikes = await Http.GetFromJsonAsync<BikeViewModel[]>("bikes")
            ?? Enumerable.Empty<BikeViewModel>();

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
        .Where(c => _warehouseFilter is null || c.WarehouseId == _warehouseFilter)
        .Where(c => _installFilter is null
            || (_installFilter == InstallState.Installed) == IsInstalled(c))
        .Where(c => _bikeFilter is null || c.CurrentPlacements.Any(p => p.BikeId == _bikeFilter))
        .Where(c => _metaFilter is null || c.IsMetaComponent == _metaFilter);

    // Installed = has any current active placement (on a bike or inside a parent component).
    private static bool IsInstalled(ComponentViewModel c) => c.ParentComponentId is not null || c.InstalledBikeCount > 0;

    private void OpenDetails(int id) => Navigation.NavigateTo($"components/{id}");

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var addDialog = await DialogService.ShowAsync<AddEditComponentDialog>("Add component", options);
        var addResult = await addDialog.Result;

        if (addResult is null || addResult.Canceled || addResult.Data is not ComponentViewModel saved)
        {
            return;
        }

        // Chain straight into installing the just-created component (Cancel leaves it created, uninstalled).
        var installParameters = new DialogParameters<InstallComponentDialog>
        {
            { x => x.ComponentId, saved.Id },
            { x => x.ComponentName, saved.Name }
        };
        var installDialog = await DialogService.ShowAsync<InstallComponentDialog>("Install component", installParameters, options);
        await installDialog.Result;

        await LoadComponentsAsync();
    }

    private async Task InstallAsync(ComponentViewModel component)
    {
        var parameters = new DialogParameters<InstallComponentDialog>
        {
            { x => x.ComponentId, component.Id },
            { x => x.ComponentName, component.Name }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<InstallComponentDialog>("Install on bike", parameters, options);
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
            await response.ShowProblemAsync(Snackbar);
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

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // A meta component may hold parts whose history a force-delete must not wipe — never force those from here
            // (a non-meta component can never be a parent, so it's safe to offer). Precise handling is on the detail page.
            if (component.IsMetaComponent)
            {
                await response.ShowProblemAsync(Snackbar);
                return;
            }

            var forceConfirmed = await DialogService.ShowMessageBoxAsync(
                "Delete component and its history?",
                $"\"{component.Name}\" has installation history. Deleting it will also remove those installation records. This cannot be undone.",
                yesText: "Delete anyway",
                cancelText: "Cancel");

            if (forceConfirmed != true)
            {
                return;
            }

            response = await Http.DeleteAsync($"components/{component.Id}?force=true");
        }

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Component deleted", Severity.Success);
            await LoadComponentsAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
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
