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
    private readonly KomTracker.Application.Services.IAthleteService _athleteService;
 
    public ClaimsFactory(
        UserManager<T> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        KomTracker.Application.Services.IAthleteService athleteService) : base(userManager, optionsAccessor)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(T user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var athlete = await _athleteService.GetAthleteAsync(user.AthleteId);

        identity.AddClaims(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
        identity.AddClaim(new Claim(Constants.Claims.AthleteId, athlete.AthleteId.ToString()));
        identity.AddClaim(new Claim(Constants.Claims.FirstName, athlete.FirstName));
        identity.AddClaim(new Claim(Constants.Claims.LastName, athlete.LastName));
        identity.AddClaim(new Claim(Constants.Claims.Avatar, athlete.ProfileMedium));

        return identity;
    }
}