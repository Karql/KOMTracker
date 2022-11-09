using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class KomsChanges
{
    private bool _loaded = false;
    private string _searchString = "";
    private IEnumerable<LastKomsChangesViewModel> _changes = Enumerable.Empty<LastKomsChangesViewModel>();
    private LastKomsChangesViewModel _change;

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
            new BreadcrumbItem("Koms changes", href: "/koms-changes"),
        };

        await GetKomsCahngesAsync();

        _loaded = true;
    }

    private async Task GetKomsCahngesAsync()
    {
        _changes = await Http.GetFromJsonAsync<LastKomsChangesViewModel[]>($"stats/koms-changes")
            ?? Enumerable.Empty<LastKomsChangesViewModel>();
    }

    private bool Search(LastKomsChangesViewModel change)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;

        return
            change.Change.Segment.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true
            || change.Athlete.FullName.Contains(_searchString, StringComparison.OrdinalIgnoreCase);

    }
}