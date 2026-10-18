using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.ServiceRecord;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Currency;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Services
{
    // localStorage key for this page's card/table preference.
    private const string ViewPreferenceKey = "services";

    private bool _loaded;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private string? _targetFilter;
    private int? _yearFilter;
    private string _currency = Currencies.Default;
    private IEnumerable<ServiceRecordViewModel> _services = Enumerable.Empty<ServiceRecordViewModel>();

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IPreferenceService Preferences { get; set; } = default!;
    [Inject] private ICurrencyPreference CurrencyPreference { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Services", href: "services"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);
        _currency = await CurrencyPreference.GetAsync();

        await LoadServicesAsync();

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

    private async Task LoadServicesAsync()
    {
        _services = await Http.GetFromJsonAsync<ServiceRecordViewModel[]>("services")
            ?? Enumerable.Empty<ServiceRecordViewModel>();
    }

    // "b:3" / "c:12" — a bike or a component, to filter by what a service covered.
    private static string Key(ServiceTargetViewModel t) => t.BikeId is not null ? $"b:{t.BikeId}" : $"c:{t.ComponentId}";

    private IEnumerable<(string Key, string Name)> TargetOptions => _services
        .SelectMany(s => s.Targets)
        .GroupBy(Key)
        .Select(g => (g.Key, Name: g.First().Name ?? g.Key))
        .OrderBy(t => t.Key.StartsWith("c:"))
        .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase);

    private IEnumerable<int> Years => _services
        .Select(s => s.Date.Year)
        .Distinct()
        .OrderByDescending(y => y);

    private bool Search(ServiceRecordViewModel service)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        return service.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || (service.Place?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || service.Targets.Any(t => t.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true);
    }

    private IEnumerable<ServiceRecordViewModel> FilteredServices => _services
        .Where(Search)
        .Where(s => _targetFilter is null || s.Targets.Any(t => Key(t) == _targetFilter))
        .Where(s => _yearFilter is null || s.Date.Year == _yearFilter);

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditServiceDialog>("Add service", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadServicesAsync();
        }
    }

    private async Task EditAsync(ServiceRecordViewModel service)
    {
        var parameters = new DialogParameters<AddEditServiceDialog> { { x => x.Service, service } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditServiceDialog>("Edit service", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadServicesAsync();
        }
    }

    private async Task DeleteAsync(ServiceRecordViewModel service)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete service",
            $"Delete \"{service.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"services/{service.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Service deleted", Severity.Success);
            await LoadServicesAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }
}
