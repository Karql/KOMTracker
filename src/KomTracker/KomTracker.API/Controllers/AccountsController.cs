using IdentityServer4.Events;
using IdentityServer4.Services;
using KomTracker.Application.Commands.Account;
using KomTracker.Application.Services;
using KomTracker.Infrastructure.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Controllers;

[Route("accounts")]
public class AccountsController : BaseApiController<AccountsController>
{
    private readonly UserManager<UserEntity> _userManager;
    private SignInManager<UserEntity> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private IEventService _events;

    public AccountsController(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, IIdentityServerInteractionService interaction, IEventService events)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        _events = events ?? throw new ArgumentNullException(nameof(events));
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(string returnUrl)
    {
        var base64RedirectUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl));

        return Redirect($"https://www.strava.com/oauth/authorize?approval_prompt=auto&scope=read,activity:read,profile:read_all&client_id=17585&response_type=code&redirect_uri=https://localhost:5001/accounts/connect&state={base64RedirectUrl}");
    }

    [HttpGet("connect")]
    public async Task<ActionResult> Connect([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
    {
        var res = await _mediator.Send(new ConnectCommand(code, scope));


        var returnUrl = Encoding.UTF8.GetString(Convert.FromBase64String(state));

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        var user = await _userManager.FindByNameAsync("karql");

        await _signInManager.SignInAsync(user, true);
        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context.Client.ClientId));

        return Redirect(returnUrl);

        // TODO: errors handling and redirects by state

        //return new CreatedResult("test", null);
    }
}