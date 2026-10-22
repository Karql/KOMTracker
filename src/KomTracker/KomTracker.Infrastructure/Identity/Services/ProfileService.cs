using IdentityServer4.Models;
using KomTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Services;

/// <summary>
/// The ASP.NET Identity profile service + "last seen": IdentityServer asks <c>IsActive</c> on every token it issues or
/// refreshes — including the silent re-logins on the IdP cookie (14 days, sliding) that never reach the Strava login —
/// so this is where an active user is noticed. Written at most once per <see cref="SeenInterval"/>.
/// </summary>
public class ProfileService : IdentityServer4.AspNetIdentity.ProfileService<UserEntity>
{
    public static readonly TimeSpan SeenInterval = TimeSpan.FromHours(1);

    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(UserManager<UserEntity> userManager, IUserClaimsPrincipalFactory<UserEntity> claimsFactory, ILogger<ProfileService> logger)
        : base(userManager, claimsFactory)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task IsActiveAsync(IsActiveContext context, UserEntity user)
    {
        await base.IsActiveAsync(context, user);

        var now = DateTime.UtcNow;
        if (!context.IsActive || user.LastSeenAt > now - SeenInterval)
        {
            return;
        }

        // Best effort: a failed write must never fail the token request.
        try
        {
            user.LastSeenAt = now;
            await _userManager.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not record last seen for user {UserId}", user.Id);
        }
    }
}
