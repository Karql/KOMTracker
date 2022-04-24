using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
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

    private IEnumerable<EffortViewModel> _newKoms = Enumerable.Empty<EffortViewModel>();
    private IEnumerable<EffortViewModel> _lostKoms = Enumerable.Empty<EffortViewModel>();

    private int _totalKomsCount = 0;
    private int _newKomsCount = 0;
    private int _lostKomsCount = 0;

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

        _newKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.NewKom).OrderByDescending(x => x.SummarySegmentEffort.KomSummaryId);
        _lostKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.LostKom).OrderByDescending(x => x.SummarySegmentEffort.KomSummaryId);

        _newKomsCount = _newKoms.Count();
        _lostKomsCount = _lostKoms.Count();
    }

    private async Task GetSummariesAsync()
    {
        _komsSummaries = await Http.GetFromJsonAsync<KomsSummaryViewModel[]>($"athletes/{_user.AthleteId}/summaries")
            ?? Enumerable.Empty<KomsSummaryViewModel>();

        var lastSummary = _komsSummaries.OrderByDescending(x => x.TrackDate).FirstOrDefault();

        _totalKomsCount = lastSummary?.Koms ?? 0;
    }
}
