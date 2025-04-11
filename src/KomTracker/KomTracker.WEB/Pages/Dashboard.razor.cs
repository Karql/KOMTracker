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
    private class ChangeViewModel
    {
        public required SegmentViewModel Segment { get; set; }
        public int KomSummaryId { get; set; }
    }

    private bool _loaded = false;
    private UserModel _user = default!;
    private IEnumerable<EffortViewModel> _lastKomsChanges = Enumerable.Empty<EffortViewModel>();
    private IEnumerable<KomsSummaryViewModel> _komsSummaries = Enumerable.Empty<KomsSummaryViewModel>();

    private List<ChangeViewModel> _newKomsChanges = new List<ChangeViewModel>();
    private List<ChangeViewModel> _lostKomsChanges = new List<ChangeViewModel>();

    private int _totalKomsCount = 0;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

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

        // calculate new and lost koms
        foreach (var segmentChanges in _lastKomsChanges.GroupBy(x => x.Segment.Id))
        {
            var lastChange = segmentChanges.OrderByDescending(e => e.SummarySegmentEffort.KomSummaryId).First();
            var anyNewInChanges = segmentChanges.Any(e => e.SummarySegmentEffort.NewKom);

            if (lastChange.SummarySegmentEffort.LostKom)
            {
                _lostKomsChanges.Add(new ChangeViewModel {
                    Segment = lastChange.Segment,
                    KomSummaryId = lastChange.SummarySegmentEffort.KomSummaryId
                });
            }

            else if (lastChange.SummarySegmentEffort.NewKom
                || (anyNewInChanges && (lastChange.SummarySegmentEffort.ImprovedKom || lastChange.SummarySegmentEffort.ReturnedKom)))
            {
                _newKomsChanges.Add(new ChangeViewModel
                {
                    Segment = lastChange.Segment,
                    KomSummaryId = lastChange.SummarySegmentEffort.KomSummaryId
                });
            }
        }
    }

    private async Task GetSummariesAsync()
    {
        _komsSummaries = await Http.GetFromJsonAsync<KomsSummaryViewModel[]>($"athletes/{_user.AthleteId}/summaries")
            ?? Enumerable.Empty<KomsSummaryViewModel>();

        var lastSummary = _komsSummaries.OrderByDescending(x => x.TrackDate).FirstOrDefault();

        _totalKomsCount = lastSummary?.Koms ?? 0;
    }
}
