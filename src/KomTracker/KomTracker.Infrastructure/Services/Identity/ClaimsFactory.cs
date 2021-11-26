using IdentityModel;
using KomTracker.Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Services.Identity;

public class ClaimsFactory<T> : UserClaimsPrincipalFactory<T>
    where T : IdentityUser
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

        return identity;
    }
}