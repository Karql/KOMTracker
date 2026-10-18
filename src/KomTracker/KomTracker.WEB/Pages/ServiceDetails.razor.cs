using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.ServiceRecord;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Currency;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class ServiceDetails
{
    [Parameter] public int Id { get; set; }

    private bool _loaded;
    private ServiceRecordViewModel? _service;
    private string _currency = Currencies.Default;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ICurrencyPreference CurrencyPreference { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _currency = await CurrencyPreference.GetAsync();

        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Services", href: "services"),
            new BreadcrumbItem("Details", href: $"services/{Id}"),
        });

        await LoadAsync();

        _loaded = true;
    }

    private async Task LoadAsync()
    {
        var response = await Http.GetAsync($"services/{Id}");

        _service = response.StatusCode == HttpStatusCode.NotFound
            ? null
            : await response.Content.ReadFromJsonAsync<ServiceRecordViewModel>();
    }

    private async Task EditAsync()
    {
        if (_service is null)
        {
            return;
        }

        var parameters = new DialogParameters<AddEditServiceDialog> { { x => x.Service, _service } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditServiceDialog>("Edit service", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task DeleteAsync()
    {
        if (_service is null)
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete service",
            $"Delete \"{_service.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"services/{_service.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Service deleted", Severity.Success);
            Navigation.NavigateTo("services");
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }
}
