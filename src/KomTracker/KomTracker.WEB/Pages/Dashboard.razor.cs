using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class Dashboard
{
    private bool _loaded = false;
    private UserModel _user = default!;
    private IEnumerable<EffortViewModel> _lastKomsChanges = Enumerable.Empty<EffortViewModel>();

    private int _newKoms = 0;
    private int _lostKoms = 0;

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _user = await UserService.GetCurrentUser();

        await Task.WhenAll(
            GetLastKomsChanges()
        );

        _loaded = true;
    }

    private async Task GetLastKomsChanges()
    {
        _lastKomsChanges = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{_user.AthleteId}/koms-changes")
            ?? Enumerable.Empty<EffortViewModel>();

        _newKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.NewKom).Count();
        _lostKoms = _lastKomsChanges.Where(x => x.SummarySegmentEffort.LostKom).Count();
    }
}
