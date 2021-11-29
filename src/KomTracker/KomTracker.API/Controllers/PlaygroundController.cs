using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using KomTracker.Application.Commands.Tracking;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using Microsoft.AspNetCore.Authorization;
using static KomTracker.Application.Constants;
using KomTracker.API.Attributes;

namespace KomTracker.API.Controllers; 

#if DEBUG
[Route("playground")]
[ApiController]
[BearerAuthorize(Roles = Roles.Admin)]
public class PlaygroundController : BaseApiController<PlaygroundController>
{
    private readonly IServiceProvider _serviceProvider;

    public PlaygroundController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    [HttpGet("test")]
    public async Task<ActionResult> Test(string token)
    {
        var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();
        var stravaAthleteService = _serviceProvider.GetRequiredService<IStravaAthleteService>();

        //var res = await athleteApi.GetKomsAsync(2394302, token);
        //var res = await athleteService.GetAthleteKomsAsync(2394302, token);

        var cancellationTokenSource = new CancellationTokenSource();

        await _mediator.Send(new TrackKomsCommand(), cancellationTokenSource.Token);

        return new NoContentResult();
    }
}
#endif
