using KomTracker.API.Shared.Helpers;
using KomTracker.API.Shared.Models.User;
using Microsoft.AspNetCore.Components.Authorization;
using IdentityConstants = KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.WEB.Infrastructure.Services.User;

public class UserService : IUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
    }

    public async Task<UserModel> GetCurrentUser()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        return UserHelper.GetUserFromPrincipal(user);
    }
}
