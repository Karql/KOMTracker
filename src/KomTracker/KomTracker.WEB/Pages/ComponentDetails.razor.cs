using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Component;
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
