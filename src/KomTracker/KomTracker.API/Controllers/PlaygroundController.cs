using Microsoft.AspNetCore.Mvc;
using Strava.API.Client.Api;
using KomTracker.Application.Commands.Tracking;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using static KomTracker.Application.Constants;
using KomTracker.API.Attributes;
using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Mail;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Notifications.Tracking;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Application.Interfaces.Services.Identity;

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
    public async Task<ActionResult> Test()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();
        var segmentApi = _serviceProvider.GetRequiredService<ISegmentApi>();
        var stravaAthleteService = _serviceProvider.GetRequiredService<IStravaAthleteService>();
        var mailService = _serviceProvider.GetRequiredService<IMailService>();
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        //var res = await athleteApi.GetKomsAsync(2394302, token);
        //var res = await athleteService.GetAthleteKomsAsync(2394302, token);

        //var res = await segmentApi.GetSegmentAsync(30393774, token);


        //await _mediator.Send(new TrackKomsCommand(), cancellationTokenSource.Token);
        //await _mediator.Send(new RefreshSegmentsCommand { SegmentsToRefresh = 2 }, cancellationTokenSource.Token);       

        return new NoContentResult();
    }
}
#endif
