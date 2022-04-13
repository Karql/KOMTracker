using KomTracker.WEB.Models.User;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
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

        return new()
        {
            AthleteId = Convert.ToInt32(user.FindFirst(IdentityConstants.Claims.AthleteId)?.Value),
            FirstName = user.FindFirst(IdentityConstants.Claims.FirstName)?.Value!,
            LastName = user.FindFirst(IdentityConstants.Claims.LastName)?.Value!,
            Username = user.FindFirst(IdentityConstants.Claims.Username)?.Value!,
            Avatar = user.FindFirst(IdentityConstants.Claims.Avatar)?.Value!,
            Email = user.FindFirst(IdentityConstants.Claims.Email)?.Value
        };
    }
}
