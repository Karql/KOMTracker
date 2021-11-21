using KomTracker.Application.Commands.Account;
using KomTracker.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.API.Controllers;

[Route("accounts")]
public class AccountsController : BaseApiController<AccountsController>
{
    [HttpGet("connect")]
    public async Task<ActionResult> Connect([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
    {
        await _mediator.Send(new ConnectCommand(code, scope));

        // TODO: errors handling and redirects by state

        return new CreatedResult("test", null);
    }
}