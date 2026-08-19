using System.Globalization;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class StravaActivities
{
    private bool _loaded;
    private StravaSyncStatusViewModel _status = new();
    private DateTime? _lastUpdated;
    private MudTable<ActivityViewModel> _table = default!;
    private long? _refreshingId;

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
            new BreadcrumbItem("Strava activities", href: "bike-tracker/strava-activities"),
        });

        _status = await Http.GetFromJsonAsync<StravaSyncStatusViewModel>("bike-tracker/strava/sync-status")
            ?? new StravaSyncStatusViewModel();

        var history = await Http.GetFromJsonAsync<ActivitySyncHistoryViewModel[]>("bike-tracker/strava/activity-sync-history?take=1")
            ?? Array.Empty<ActivitySyncHistoryViewModel>();
        _lastUpdated = history.FirstOrDefault()?.RunAt;

        _loaded = true;
    }

    private async Task<TableData<ActivityViewModel>> LoadServerData(TableState state, CancellationToken token)
    {
        var qParams = new Dictionary<string, string?>
        {
            ["page"] = state.Page.ToString(),
            ["pageSize"] = state.PageSize.ToString()
        };

        var result = await Http.GetFromJsonAsync<PagedResultViewModel<ActivityViewModel>>(
            QueryHelpers.AddQueryString("bike-tracker/strava/activities", qParams), token);

        return new TableData<ActivityViewModel>
        {
            Items = result?.Items ?? Array.Empty<ActivityViewModel>(),
            TotalItems = result?.TotalCount ?? 0
        };
    }

    private async Task RefreshAsync(long id)
    {
        _refreshingId = id;
        try
        {
            var response = await Http.PostAsync($"bike-tracker/strava/activities/{id}/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Activity refreshed", Severity.Success);
                await _table.ReloadServerData();
            }
            else
            {
                Snackbar.Add($"Refresh failed ({(int)response.StatusCode}).", Severity.Error);
            }
        }
        finally
        {
            _refreshingId = null;
        }
    }

    private async Task ShowHistoryAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
        await DialogService.ShowAsync<StravaSyncHistoryDialog>("Activity sync history", options);
    }

    private void GoToAccount() => Navigation.NavigateTo("account?tab=strava");

    private void GoToStravaBikes() => Navigation.NavigateTo("bike-tracker/strava-bikes");

    private void OpenBike(int id) => Navigation.NavigateTo($"bikes/{id}");

    private static string FormatDate(DateTime local) => local.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private static string FormatMovingTime(int seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return (int)ts.TotalHours > 0 ? $"{(int)ts.TotalHours}h {ts.Minutes}m" : $"{ts.Minutes}m";
    }

    private static string ActivityUrl(long id) => $"https://www.strava.com/activities/{id}";
}
