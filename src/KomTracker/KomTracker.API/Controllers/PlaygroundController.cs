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
using KomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using KomTracker.API.Shared.Helpers;
using KomTracker.Application.Commands.Stats;
using KomTracker.Application.Queries.Stats;

namespace KomTracker.API.Controllers; 

//#if DEBUG
[Route("playground")]
[ApiController]
[BearerAuthorize(Roles = Roles.Admin)]
public class PlaygroundController : BaseApiController<PlaygroundController>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KOMDBContext _context;

    public PlaygroundController(IServiceProvider serviceProvider, KOMDBContext context)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpPost("test")]
    public async Task<ActionResult> Test(string token)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();
        var segmentApi = _serviceProvider.GetRequiredService<ISegmentApi>();
        var clubApi    = _serviceProvider.GetRequiredService<IClubApi>();
        var stravaAthleteService = _serviceProvider.GetRequiredService<IStravaAthleteService>();
        var mailService = _serviceProvider.GetRequiredService<IMailService>();
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        //var res = await athleteApi.GetKomsAsync(2394302, token);
        //var res = await athleteService.GetAthleteKomsAsync(2394302, token);

        //var res = await segmentApi.GetSegmentAsync(30393774, token);


        //await _mediator.Send(new TrackKomsCommand(), cancellationTokenSource.Token);
        //await _mediator.Send(new RefreshSegmentsCommand { SegmentsToRefresh = 2 }, cancellationTokenSource.Token);
        //await _mediator.Send(new RecalculateStatsCommand(), cancellationTokenSource.Token); 

        var res = await _mediator.Send(new GetLastKomsChangesQuery());

        //var clubs = await clubApi.GetClubsAsync(token);



        return new NoContentResult();
    }

    [HttpGet("fix-returned-koms")]
    public async Task<ActionResult> FixReturnedKoms()
    {
        var newEfforts = await _context.KomsSummarySegmentEffort
            .Where(x => x.NewKom)
            .OrderBy(x => x.KomSummaryId)
            .ToListAsync();

        foreach (var newEffort in newEfforts)
        {
            // check is exists before
            var exists = await _context.KomsSummarySegmentEffort.AnyAsync(x => x.KomSummaryId < newEffort.KomSummaryId && x.SegmentEffortId == newEffort.SegmentEffortId);

            // modify
            if (exists)
            {
                var komSummary = await _context.KomsSummary.FirstOrDefaultAsync(x => x.Id == newEffort.KomSummaryId);

                newEffort.NewKom = false;
                newEffort.ReturnedKom = true;
                
                komSummary.NewKoms--;
                komSummary.ReturnedKoms++;

                _context.SaveChanges();
            }
        }

        return new NoContentResult();
    }

    [HttpGet("test-polyline")]
    public async Task<ActionResult> TestPolyline()
    {
        var polyline = @"uzdpHun}wBu@VWQi@i@sAiB[Wi@[a@KSJu@d@i@Vo@`@a@`BYj@W`AYt@a@p@UVa@\u@\iCPyAA"; // bogucianka
        var points = MapHelper.Decode(polyline);

        return new NoContentResult();
    }
}
//#endif
