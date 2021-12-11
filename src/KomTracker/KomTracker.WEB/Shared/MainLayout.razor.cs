using KomTracker.WEB.Services.Preference;
using KomTracker.WEB.Settings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Shared;

public partial class MainLayout
{
    private bool _drawerOpen = true;
    private MudTheme _currentTheme = Theme.DefaultTheme;

    [Inject]
    private IPreferenceService PreferenceService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Dashboard", href: "#"),
    };

    private async Task ToggleDarkModeAsync()
    {
        await PreferenceService.ToggleDarkModeAsync();
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
    }
}
