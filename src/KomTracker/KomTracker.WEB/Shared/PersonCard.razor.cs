﻿using KomTracker.API.Shared.Models.User;
using KomTracker.WEB.Infrastructure.Services.User;
using Microsoft.AspNetCore.Components;

namespace KomTracker.WEB.Shared;

public partial class PersonCard
{
    [Parameter] public required string Class { get; set; }
    [Parameter] public required string Style { get; set; }

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
