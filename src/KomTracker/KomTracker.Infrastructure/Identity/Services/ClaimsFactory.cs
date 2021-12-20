using IdentityModel;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Services;

public class ClaimsFactory<T> : UserClaimsPrincipalFactory<T>
    where T : UserEntity
{
    private readonly UserManager<T> _userManager;

    public ClaimsFactory(
        UserManager<T> userManager,
        IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
        _userManager = userManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(T user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        identity.AddClaims(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));

        identity.AddClaim(new Claim(Constants.Claims.AthleteId, user.AthleteId.ToString()));

        return identity;
    }
}