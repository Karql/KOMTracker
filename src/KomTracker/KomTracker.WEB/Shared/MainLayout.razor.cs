using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using KomTracker.WEB.Settings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Shared;

public partial class MainLayout
{
    private UserModel _user = new ();
    private bool _drawerOpen = true;
    private MudTheme _currentTheme = Theme.DefaultTheme;

    [Inject]
    private IPreferenceService PreferenceService { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
        _user = await UserService.GetCurrentUser();
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Koms", href: "#"),
    };

    private async Task ToggleDarkModeAsync()
    {
        await PreferenceService.ToggleDarkModeAsync();
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
    }
}
