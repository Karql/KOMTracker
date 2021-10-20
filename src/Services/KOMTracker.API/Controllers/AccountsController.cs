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

        public AccountsController(ITokenApi tokenApi)
        {
            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
        }

        [HttpGet("connect")]
        public async Task<ActionResult> Connect([FromQuery]string code, [FromQuery]string scope, [FromQuery]string state)
        {
            // 1. verify scope
            
            // 2. exchange token
            var exchangeResult = await _tokenApi.ExchangeAsync(code);

            if (!exchangeResult.IsSuccess)
            {
                // TODO: 
                // - better error handling
                throw new Exception($"{nameof(_tokenApi.ExchangeAsync)} failed!");
            }

            var tokenWithAthlete = exchangeResult.Value;

            // 3. add athlete and user when not exists
            // 4. login 

            return new  OkObjectResult(null);
        }
    }
}
