using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class Dashboard
{
    private bool _loaded = false;
    private UserModel _user = default!;
    private IEnumerable<EffortViewModel> _lastKomsChanges = Enumerable.Empty<EffortViewModel>();
    private IEnumerable<KomsSummaryViewModel> _komsSummaries = Enumerable.Empty<KomsSummaryViewModel>();

    private int _totalKoms = 0;
    private int _newKoms = 0;
    private int _lostKoms = 0;

    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Dashboard", href: ""),
        };

        _user = await UserService.GetCurrentUser();

        await Task.WhenAll(
            GetLastKomsChangesAsync(),
            GetSummariesAsync()
        );

        _loaded = true;
    }

    private async Task GetLastKomsChangesAsync()
    {
        _lastKomsChanges = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{_user.AthleteId}/koms-changes")
            ?? Enumerable.Empty<EffortViewModel>();

        _newKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.NewKom).Count();
        _lostKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.LostKom).Count();
    }

    private async Task GetSummariesAsync()
    {
        _komsSummaries = await Http.GetFromJsonAsync<KomsSummaryViewModel[]>($"athletes/{_user.AthleteId}/summaries")
            ?? Enumerable.Empty<KomsSummaryViewModel>();

        var lastSummary = _komsSummaries.OrderByDescending(x => x.TrackDate).FirstOrDefault();

        _totalKoms = lastSummary?.Koms ?? 0;
    }
}
