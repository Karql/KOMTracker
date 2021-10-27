using KOMTracker.API.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Controllers
{
    [Route("accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ITokenApi _tokenApi;
        private readonly IAccountService _accountService;

        public AccountsController(ITokenApi tokenApi, IAccountService athleteService)
        {
            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
            _accountService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        }

        [HttpGet("connect")]
        public async Task<ActionResult> Connect([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
        {
            await _accountService.Connect(code, scope);

            return new  OkObjectResult(null);
        }
    }
}
