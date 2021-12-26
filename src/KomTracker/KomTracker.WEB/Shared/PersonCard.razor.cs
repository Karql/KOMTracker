using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Models.User;
using Microsoft.AspNetCore.Components;

namespace KomTracker.WEB.Shared;

public partial class PersonCard
{
    [Parameter] public string Class { get; set; }
    [Parameter] public string Style { get; set; }

    private bool _loaded = false;
    private UserModel _user = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        _user = await UserService.GetCurrentUser();
        _loaded = true;
    }
}
