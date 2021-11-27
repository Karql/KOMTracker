using System;
using System.Collections.Generic;
using System.Linq;
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
}
