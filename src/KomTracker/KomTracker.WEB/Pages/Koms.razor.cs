using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using KomTracker.WEB.Shared;
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
            new BreadcrumbItem("Koms", href: "/koms"),
        };

        _user = await UserService.GetCurrentUser();

        await GetAllKoms();

        _loaded = true;
    }

    private async Task GetAllKoms()
    {
        _koms = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{_user.AthleteId}/koms")
            ?? Enumerable.Empty<EffortViewModel>();
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