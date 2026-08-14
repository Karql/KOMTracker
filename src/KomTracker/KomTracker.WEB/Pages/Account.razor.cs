using System.Net.Http.Json;
using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Account
{
    private const int StravaTabIndex = 1;

    private bool _loaded = false;
    private UserModel _user = default!;
    private StravaSyncStatusViewModel _stravaStatus = new();
    private int _activeTab;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    //public bool KomsChangesNotification { get; set; } = true;
    //public bool NotificationEmail_2 { get; set; }
    //public bool NotificationEmail_3 { get; set; }
    //public bool NotificationEmail_4 { get; set; } = true;

    private bool _profileDetailsValid = false;
    private string? _email;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Account", href: "account"),
        });

        _user = await UserService.GetCurrentUser();

        _email = _user.Email;

        await LoadStravaStatusAsync();

        HandleQueryParams();

        _loaded = true;
    }

    private async Task LoadStravaStatusAsync()
    {
        _stravaStatus = await Http.GetFromJsonAsync<StravaSyncStatusViewModel>("bike-tracker/strava/sync-status")
            ?? new StravaSyncStatusViewModel();
    }

    private void HandleQueryParams()
    {
        var query = QueryHelpers.ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);

        if (query.TryGetValue("tab", out var tab) && tab == "strava")
        {
            _activeTab = StravaTabIndex;
        }

        if (query.TryGetValue("strava_upgrade", out var upgrade))
        {
            var intent = query.TryGetValue("intent", out var i) ? i.ToString() : null;

            if (upgrade != "ok")
            {
                Snackbar.Add("Strava access change failed. Please try again.", Severity.Error);
            }
            else if (intent == "revoke")
            {
                Snackbar.Add(_stravaStatus.HasActivityReadAll
                    ? "Strava access updated."
                    : "Private rides access revoked.", Severity.Success);
            }
            else if (intent == "allow")
            {
                Snackbar.Add(_stravaStatus.HasActivityReadAll
                    ? "Private rides access granted."
                    : "Private rides access wasn't granted — public rides only.",
                    _stravaStatus.HasActivityReadAll ? Severity.Success : Severity.Warning);
            }
            else
            {
                Snackbar.Add("Strava access updated.", Severity.Success);
            }
        }
    }

    private void AllowPrivateRides() => ChangeStravaAccess(mode: "full", intent: "allow");

    private void RevokePrivateRides() => ChangeStravaAccess(mode: "basic", intent: "revoke");

    private void ChangeStravaAccess(string mode, string intent)
    {
        var authority = Configuration["IdentityConfiguration:Authority"];
        var returnUrl = Navigation.ToAbsoluteUri($"account?tab=strava&intent={intent}").ToString();
        var upgradeUrl = $"{authority}/account/upgrade?mode={mode}&returnUrl={Uri.EscapeDataString(returnUrl)}";

        Navigation.NavigateTo(upgradeUrl, forceLoad: true);
    }

    private async Task ChangeEmailAsync()
    {
        if (_email == _user.Email)
        {
            Snackbar.Add("You have provided the same email.", Severity.Warning, config =>
            {
                config.ShowCloseIcon = false;
            });
            return;
        }

        var res = await Http.PutAsync($"athletes/{_user.AthleteId}/change-email/{_email}", null);

        if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            Snackbar.Add("Email change confirmation has been sent. Please check your inbox.", Severity.Success, config =>
            {
                config.ShowCloseIcon = false;
            });
        }

        else
        {
            Snackbar.Add("Something went wrong :(", Severity.Error, config =>
            {
                config.ShowCloseIcon = false;
            });
        }
    }
}