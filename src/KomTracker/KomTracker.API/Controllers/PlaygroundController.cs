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
        var cancellationTokenSource = new CancellationTokenSource();

        var athleteApi = _serviceProvider.GetRequiredService<IAthleteApi>();
        var segmentApi = _serviceProvider.GetRequiredService<ISegmentApi>();
        var stravaAthleteService = _serviceProvider.GetRequiredService<IStravaAthleteService>();
        var mailService = _serviceProvider.GetRequiredService<IMailService>();

        //var res = await athleteApi.GetKomsAsync(2394302, token);
        //var res = await athleteService.GetAthleteKomsAsync(2394302, token);

        //var res = await segmentApi.GetSegmentAsync(30393774, token);


        await _mediator.Send(new TrackKomsCommand(), cancellationTokenSource.Token);
        //await _mediator.Send(new RefreshSegmentsCommand { SegmentsToRefresh = 2 }, cancellationTokenSource.Token);
        //await _mediator.Publish(new TrackKomsCompletedNotification
        //{
        //    Athlete = new AthleteEntity
        //    {
        //        AthleteId = 2394302,
        //        FirstName = "Mateusz"
        //    },
        //    ComparedEfforts = new ComparedEffortsModel
        //    {
        //        KomsCount = 625,
        //        NewKomsCount = 1,
        //        ImprovedKomsCount = 1,
        //        LostKomsCount = 1,
        //        Efforts = new List<EffortModel>
        //        {
        //            new EffortModel
        //            {
        //                Segment = new Domain.Entities.Segment.SegmentEntity
        //                {
        //                    Id = 1,
        //                    Name = "Seg1"
        //                },
        //                SummarySegmentEffort = new Domain.Entities.Segment.KomsSummarySegmentEffortEntity
        //                {
        //                    NewKom = true
        //                }
        //            },
        //            new EffortModel
        //            {
        //                Segment = new Domain.Entities.Segment.SegmentEntity
        //                {
        //                    Id = 2,
        //                    Name = "Seg2"
        //                },
        //                SummarySegmentEffort = new Domain.Entities.Segment.KomsSummarySegmentEffortEntity
        //                {
        //                    ImprovedKom = true
        //                }
        //            },
        //            new EffortModel
        //            {
        //                Segment = new Domain.Entities.Segment.SegmentEntity
        //                {
        //                    Id = 3,
        //                    Name = "Seg3"
        //                },
        //                SummarySegmentEffort = new Domain.Entities.Segment.KomsSummarySegmentEffortEntity
        //                {
        //                    LostKom = true
        //                }
        //            }
        //        }
        //    }
        //});

        return new NoContentResult();
    }
}
#endif
