using KomTracker.API.Shared.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityConstants = KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.API.Shared.Helpers;
public static class UserHelper
{
    public static UserModel GetUserFromPrincipal(ClaimsPrincipal user)
    {
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
