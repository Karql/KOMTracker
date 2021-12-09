using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KomTracker.WEB.Pages;

public partial class Koms
{
    private bool _loaded = false;
    private string _searchString = "";
    private IEnumerable<KomViewModel> _koms;
    private KomViewModel _kom;
    protected override async Task OnInitializedAsync()
    {
        _koms = Enumerable.Range(1, 100).Select(x => new KomViewModel
        {
            SegmentId = x,
            Name = $"Segment{x}"
        }).ToList();          

        _loaded = true;
    }

    private bool Search(KomViewModel kom)
    {
        if (string.IsNullOrWhiteSpace(_searchString)) return true;
        if (kom.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }
        return false;
    }
}

public class KomViewModel
{
    public long SegmentId { get; set; }

    public string Name { get; set; }
}