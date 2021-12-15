using KomTracker.API.Shared.ViewModels.Segment;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KomTracker.WEB.Pages;

public partial class Koms
{
    private bool _loaded = false;
    private string _searchString = "";
    private IEnumerable<SegmentEffortViewModel> _koms = Enumerable.Empty<SegmentEffortViewModel>();
    private SegmentEffortViewModel _kom;

    [Inject]
    private HttpClient Http { get; set; } 
    protected override async Task OnInitializedAsync()
    {
        _koms = await Http.GetFromJsonAsync<SegmentEffortViewModel[]>("athletes/2394302/koms")
            ?? Enumerable.Empty<SegmentEffortViewModel>();

        _loaded = true;
    }

    private bool Search(SegmentEffortViewModel kom)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;
        if (kom.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }
        return false;
    }
}