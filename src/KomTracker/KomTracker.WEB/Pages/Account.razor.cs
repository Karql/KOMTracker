using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KomTracker.WEB.Pages;

public partial class Account
{
    private bool _loaded = false;
    private UserModel _user = default!;

    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; }

    [Inject]
    private IUserService UserService { get; set; } = default!;

    public bool KomsChangesNotification { get; set; } = true;
    public bool NotificationEmail_2 { get; set; }
    public bool NotificationEmail_3 { get; set; }
    public bool NotificationEmail_4 { get; set; } = true;

    public string? Email { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Account", href: "/account"),
        };

        _user = await UserService.GetCurrentUser();

        Email = _user.Email;

        _loaded = true;
    }

    private void SaveChanges(string message, Severity severity)
    {
        Snackbar.Add(message, severity, config =>
        {
            config.ShowCloseIcon = false;
        });
    }
}