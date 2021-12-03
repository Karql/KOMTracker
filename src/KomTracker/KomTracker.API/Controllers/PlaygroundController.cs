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
using KomTracker.Application.Interfaces.Persistence;
using Utils.UnitOfWork.Concrete;
using KomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using MoreLinq;
using KomTracker.Domain.Entities.Segment;

namespace KomTracker.API.Controllers; 

[Route("playground")]
[ApiController]
//[BearerAuthorize(Roles = Roles.Admin)]
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

    [HttpGet("migrate-links-to-new-structure")]
    public async Task<ActionResult> MigrateLinksToNewStructure()
    {
        var _komUoW = _serviceProvider.GetRequiredService<IKOMUnitOfWork>();
        var segmentService = _serviceProvider.GetRequiredService<ISegmentService>();

        var migrateLinksRepo = _komUoW.GetRepository<MigrateLinksRepo>();
        

        await migrateLinksRepo.MigrateLinks(segmentService);


        return new NoContentResult();
    }
}

public class MigrateLinksRepo : EFRepositoryBase<KOMDBContext>
{
    public async Task MigrateLinks(ISegmentService segmentService)
    {
        var athleteIds = await _context.Athlete.Select(x => x.AthleteId).ToArrayAsync();

        foreach (var athleteId in athleteIds)
        {

            var komsSummariesIds = await _context.KomsSummary.Where(x => x.AthleteId == athleteId).Select(x => x.Id).ToArrayAsync();

            for (int i = 0; i < komsSummariesIds.Length; i++)
            {
                var lastKomSummaryId = i == 0 ? -1 : komsSummariesIds[i - 1];                               
                var actualKomSummaryId = komsSummariesIds[i];

                var lastKomsSummaryEfforts = await (
                    from ksse in _context.KomsSummarySegmentEffort
                    join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
                    where ksse.KomSummaryId == lastKomSummaryId
                    select new SegmentEffortWithLinkToKomsSummaryModel
                    {
                        SegmentEffort = se,
                        Link = ksse
                    }
                ).ToListAsync();
                var lastKomsEfforts = lastKomsSummaryEfforts.Where(x => x.Link.Kom).Select(x => x.SegmentEffort);

                var actualKoms = await (
                    from ksse in _context.KomsSummarySegmentEffort
                    join se in _context.SegmentEffort on ksse.SegmentEffortId equals se.Id
                    where ksse.KomSummaryId == actualKomSummaryId
                    select se
                ).ToListAsync();

                var comparedEfforts = segmentService.CompareEfforts(actualKoms, lastKomsEfforts);

                var linksToUpdate = comparedEfforts.EffortsWithLinks.Select(x => x.Link);

                linksToUpdate.ForEach(x => x.KomSummaryId = actualKomSummaryId);

                await _context
                    .KomsSummarySegmentEffort
                    .UpsertRange(linksToUpdate)
                    .On(x => new { x.KomSummaryId, x.SegmentEffortId })
                    .RunAsync();
            }
        }
    }
}