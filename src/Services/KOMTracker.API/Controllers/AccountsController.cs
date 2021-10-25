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
        private readonly IAthleteService _athleteService;

        public AccountsController(ITokenApi tokenApi, IAthleteService athleteService)
        {
            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
            _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
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

            // 3. check is athlete exists
            var athlete = await _athleteService.GetAthleteByIdAsync(tokenWithAthlete.Athlete.Id);

            

            // 3. add athlete and user when not exists
            // 4. login 

            return new  OkObjectResult(null);
        }
    }
}
