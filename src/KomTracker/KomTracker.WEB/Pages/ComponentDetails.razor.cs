using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Component;
using KomTracker.API.Shared.ViewModels.Installation;
using KomTracker.Domain.Entities.Component;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class ComponentDetails
{
    [Parameter] public int Id { get; set; }

    private bool _loaded;
    private ComponentViewModel? _component;
    private IReadOnlyList<InstallationViewModel> _installations = Array.Empty<InstallationViewModel>();
    private InstallationViewModel? _current;

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
            new BreadcrumbItem("Components", href: "components"),
            new BreadcrumbItem("Details", href: $"components/{Id}"),
        });

        await LoadAsync();

        _loaded = true;
    }

    private async Task LoadAsync()
    {
        var response = await Http.GetAsync($"components/{Id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _component = null;
            return;
        }

        _component = await response.Content.ReadFromJsonAsync<ComponentViewModel>();

        var installations = await Http.GetFromJsonAsync<InstallationViewModel[]>($"installations?componentId={Id}")
            ?? Array.Empty<InstallationViewModel>();

        // Display order: current first, then Tracked history (newest first), then Manual (dateless) at the end.
        _installations = installations
            .OrderByDescending(i => i.IsCurrent)
            .ThenBy(i => i.Type == ComponentInstallationType.Manual)
            .ThenByDescending(i => i.DateFrom)
            .ToArray();
        _current = _installations.FirstOrDefault(i => i.IsCurrent);
    }

    private async Task InstallAsync()
    {
        if (_component is null)
        {
            return;
        }

        var parameters = new DialogParameters<InstallComponentDialog>
        {
            { x => x.ComponentId, _component.Id },
            { x => x.ComponentName, _component.Name }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<InstallComponentDialog>("Install on bike", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task AddManualAsync()
    {
        if (_component is null)
        {
            return;
        }

        var parameters = new DialogParameters<InstallComponentDialog>
        {
            { x => x.ComponentId, _component.Id },
            { x => x.ComponentName, _component.Name },
            { x => x.DefaultType, ComponentInstallationType.Manual }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<InstallComponentDialog>("Add manual usage", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task MoveAsync(InstallationViewModel installation)
    {
        var parameters = new DialogParameters<MoveInstallationDialog> { { x => x.Installation, installation } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<MoveInstallationDialog>("Move component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task RemoveAsync(InstallationViewModel installation)
    {
        var parameters = new DialogParameters<RemoveInstallationDialog> { { x => x.Installation, installation } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<RemoveInstallationDialog>("Remove component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task EditInstallationAsync(InstallationViewModel installation)
    {
        var parameters = new DialogParameters<EditInstallationDialog> { { x => x.Installation, installation } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<EditInstallationDialog>("Edit installation", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task DeleteInstallationAsync(InstallationViewModel installation)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete installation record",
            "Delete this installation record? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"installations/{installation.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Installation record deleted", Severity.Success);
            await LoadAsync();
        }
        else
        {
            Snackbar.Add($"Delete failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task EditAsync()
    {
        if (_component is null)
        {
            return;
        }

        var parameters = new DialogParameters<AddEditComponentDialog> { { x => x.Component, _component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditComponentDialog>("Edit component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task SellAsync()
    {
        if (_component is null)
        {
            return;
        }

        var parameters = new DialogParameters<SellComponentDialog> { { x => x.Component, _component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellComponentDialog>("Sell component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task ArchiveAsync()
    {
        if (_component is null)
        {
            return;
        }

        var parameters = new DialogParameters<ArchiveComponentDialog> { { x => x.Component, _component } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<ArchiveComponentDialog>("Archive component", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private Task ActivateAsync() => ChangeLifecycleAsync(ComponentLifecycle.Active);

    private async Task ChangeLifecycleAsync(ComponentLifecycle lifecycle)
    {
        if (_component is null)
        {
            return;
        }

        var body = new ChangeComponentLifecycleViewModel { Lifecycle = lifecycle };
        var response = await Http.PutAsJsonAsync($"components/{_component.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add($"Component {lifecycle.ToString().ToLowerInvariant()}", Severity.Success);
            await LoadAsync();
        }
        else
        {
            Snackbar.Add($"Failed ({(int)response.StatusCode}).", Severity.Error);
        }
    }

    private async Task DeleteAsync()
    {
        if (_component is null)
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete component",
            $"Delete \"{_component.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"components/{_component.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Component deleted", Severity.Success);
            Navigation.NavigateTo("components");
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
