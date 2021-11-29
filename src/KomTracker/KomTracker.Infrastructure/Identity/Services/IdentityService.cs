using IdentityServer4.Events;
using IdentityServer4.Services;
using KomTracker.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Services;

internal class IdentityService : IIdentityService
{
    private readonly UserManager<UserEntity> _userManager;
    private SignInManager<UserEntity> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private IEventService _events;

    public IdentityService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, IIdentityServerInteractionService interaction, IEventService events)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        _events = events ?? throw new ArgumentNullException(nameof(events));
    }

    public async Task<string> LoginAsync(int athleteId, string state)
    {
        var returnUrl = Encoding.UTF8.GetString(Convert.FromBase64String(state));
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        if (context != null)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.AthleteId == athleteId);

            if (user == null)
            {
                throw new Exception($"User not found for AthleteId: {athleteId}!");
            }

            await _signInManager.SignInAsync(user, true);
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context.Client.ClientId));
        }

        return returnUrl;
    }
}
