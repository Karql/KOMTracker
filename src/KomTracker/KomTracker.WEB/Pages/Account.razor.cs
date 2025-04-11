using KomTracker.API.Shared.Models.User;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Account
{
    private bool _loaded = false;
    private UserModel _user = default!;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    //public bool KomsChangesNotification { get; set; } = true;
    //public bool NotificationEmail_2 { get; set; }
    //public bool NotificationEmail_3 { get; set; }
    //public bool NotificationEmail_4 { get; set; } = true;

    private bool _profileDetailsValid = false;
    private string? _email;

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Account", href: "/account"),
        };

        _user = await UserService.GetCurrentUser();

        _email = _user.Email;

        _loaded = true;
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