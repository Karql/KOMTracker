using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KOMTracker.API.Infrastructure.Services;
using System.Threading;

namespace KOMTracker.API.Controllers; 

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
    public async Task<ActionResult> Test(string token)
    {
        var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();
        var athleteService = _serviceProvider.GetRequiredService<IAthleteService>();
        var komService = _serviceProvider.GetRequiredService<IKomService>();

        //var res = await athleteApi.GetKomsAsync(2394302, token);
        //var res = await athleteService.GetAthleteKomsAsync(2394302, token);

        var cancellationTokenSource = new CancellationTokenSource();

        await komService.TrackKomsAsync(cancellationTokenSource.Token);

        return new NoContentResult();
    }
}
#endif
