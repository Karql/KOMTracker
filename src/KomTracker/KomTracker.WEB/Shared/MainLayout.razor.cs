using KomTracker.API.Shared.Models.User;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace KomTracker.WEB.Shared;

public partial class MainLayout : IDisposable
{
    private UserModel _user = new ();
    private bool _drawerOpen = true;
    private bool _isDarkMode = false;
    private ErrorBoundary? _errorBoundary;

    [Inject]
    private IPreferenceService PreferenceService { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    public List<BreadcrumbItem> BreadCrumbs = new List<BreadcrumbItem>();

    protected override async Task OnInitializedAsync()
    {
        Navigation.LocationChanged += OnLocationChanged;

        _isDarkMode = await PreferenceService.IsDarkModeAsync();
        _user = await UserService.GetCurrentUser();
    }

    // Clear a caught error when navigating (incl. the re-login redirect) so the app recovers.
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _errorBoundary?.Recover();
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private async Task ToggleDarkModeAsync()
    {
        await PreferenceService.ToggleDarkModeAsync();
        _isDarkMode = await PreferenceService.IsDarkModeAsync();
    }

    protected void LogOut(MouseEventArgs args)
    {
        Navigation.NavigateToLogout("authentication/logout");
    }
}
