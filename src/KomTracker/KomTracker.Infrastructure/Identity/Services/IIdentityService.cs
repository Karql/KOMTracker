using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Services;

internal interface IIdentityService
{
    /// <summary>
    /// Login user by AthleteId
    /// </summary>
    /// <param name="athleteId"></param>
    /// <param name="state"></param>
    /// <returns>RedirectUrl to application</returns>
    Task<string> LoginAsync(int athleteId, string state);

    /// <summary>
    /// Logout user
    /// </summary>
    /// <param name="logoutId"></param>
    /// <param name="user"></param>
    /// <returns>RedirectUrl to application</returns>
    Task<string> LogoutAsync(string logoutId, ClaimsPrincipal user);
}
