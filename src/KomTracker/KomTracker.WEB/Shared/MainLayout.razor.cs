﻿using KomTracker.API.Shared.Models.User;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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

    [Inject] NavigationManager Navigation { get; set; }
    [Inject] SignOutSessionStateManager SignOutManager { get; set; }

    public List<BreadcrumbItem> BreadCrumbs = new List<BreadcrumbItem>();

    protected override async Task OnInitializedAsync()
    {
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
        _user = await UserService.GetCurrentUser();
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private async Task ToggleDarkModeAsync()
    {
        await PreferenceService.ToggleDarkModeAsync();
        _currentTheme = await PreferenceService.GetCurrentThemeAsync();
    }

    protected async Task LogOut(MouseEventArgs args)
    {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }
}
