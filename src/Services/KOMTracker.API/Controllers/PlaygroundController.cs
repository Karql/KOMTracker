using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace KOMTracker.API.Controllers
{
#if DEBUG
    [Route("playground")]
    [ApiController]
    public class PlaygroundController
    {
        private readonly IServiceProvider _serviceProvider;

        public PlaygroundController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [HttpGet("test")]
        public async Task<ActionResult> Test()
        {
            var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();

            var res = await athleteApi.GetKomsAsync("b8d24b0f65ac59408c556f06f3ebfc78cf998727");

            return new NoContentResult();
        }
    }
#endif
}
