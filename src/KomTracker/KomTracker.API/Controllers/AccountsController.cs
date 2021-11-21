using KomTracker.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.API.Controllers;

[Route("accounts")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(ITokenApi tokenApi, IAccountService athleteService)
    {
        _accountService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    [HttpGet("connect")]
    public async Task<ActionResult> Connect([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
    {
        await _accountService.Connect(code, scope);

        // TODO: errors handling and redirects by state

        return new CreatedResult("test", null);
    }
}