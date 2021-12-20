using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KomTracker.WEB.Pages;

public partial class Koms
{
    private bool _loaded = false;
    private UserModel _user = default!;
    private string _searchString = "";
    private IEnumerable<EffortViewModel> _koms = Enumerable.Empty<EffortViewModel>();
    private EffortViewModel _kom;

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        _user = await UserService.GetCurrentUser();

        _koms = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{_user.AthleteId}/koms")
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

    private string GetActivityTypeIcon(string activityType)
    {
        return activityType switch
        {
            "Ride" => Icons.Material.Filled.DirectionsBike,
            "Run" => Icons.Material.Filled.DirectionsRun,
            "Hike" => Icons.Material.Filled.Hiking,
            "NordicSki" => Icons.Material.Filled.NordicWalking,
            _ => Icons.Material.Filled.HelpCenter
        };            
    }

    private string GetClimbCategoryColor(int climbCategory)
    {
        return climbCategory switch
        {
            -1 => "#000",
            0 => "#F3A73B",
            1 => "#EB9138",
            2 => "#E47B34",
            3 => "#DC6531",
            4 => "#D34B2D",
            5 => "#CA2A2A",
            _ => throw new ArgumentOutOfRangeException($"{nameof(climbCategory)} should has value between -1 and 5"),
        };
    }
}