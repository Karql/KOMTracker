using KomTracker.WEB.Settings;
using MudBlazor;

namespace KomTracker.WEB.Shared;

public partial class MainLayout
{
    bool _drawerOpen = true;
    MudTheme _currentTheme = Theme.DefaultTheme;

    protected override async Task OnInitializedAsync()
    {
        _currentTheme = Theme.DefaultTheme;
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Dashboard", href: "#"),
    };

    private async Task DarkMode()
    {
        _currentTheme = _currentTheme == Theme.DefaultTheme ? Theme.DarkTheme : Theme.DefaultTheme;
    }
}
