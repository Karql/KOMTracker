using KomTracker.API.Shared.ViewModels.Segment;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KomTracker.WEB.Pages;

public partial class Koms
{
    private bool _loaded = false;
    private string _searchString = "";
    private IEnumerable<EffortViewModel> _koms = Enumerable.Empty<EffortViewModel>();
    private EffortViewModel _kom;

    [Inject]
    private HttpClient Http { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        _koms = await Http.GetFromJsonAsync<EffortViewModel[]>("athletes/2394302/koms")
            ?? Enumerable.Empty<EffortViewModel>();

        _loaded = true;
    }

    private bool Search(EffortViewModel kom)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;
        if (kom.Segment.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }
        return false;
    }
}