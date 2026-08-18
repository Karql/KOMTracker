using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Infrastructure.Jobs;
using KomTracker.API.Shared.ViewModels;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Queries.Strava;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("bike-tracker/strava")]
[ApiController]
[BearerAuthorize()]
public class StravaBikesController : BaseApiController<StravaBikesController>
{
    /// <summary>Manual Strava bike (gear) sync — quick, always available; does not change the auto-sync flag.</summary>
    [HttpPost]
    [Route("sync-bikes")]
    public async Task<IActionResult> SyncBikes()
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SyncStravaBikesCommand { AthleteId = user.AthleteId });

        return this.ToActionResult(result);
    }

    /// <summary>Enable/disable automatic activity sync. First-ever enable kicks a background full backfill.</summary>
    [HttpPut]
    [Route("activity-sync")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ActivitySyncToggleResultViewModel))]
    public async Task<IActionResult> SetActivitySync([FromQuery] bool enabled)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SetActivitySyncCommand { AthleteId = user.AthleteId, Enabled = enabled });
        if (result.IsFailed)
        {
            return this.ToActionResult(result, _ => (object?)null);
        }

        var backfillStarted = result.Value.BackfillNeeded && await ScheduleBackfillAsync(user.AthleteId);

        return Ok(new ActivitySyncToggleResultViewModel { BackfillStarted = backfillStarted });
    }

    /// <summary>Enable/disable automatic bike (gear) sync (gates the bike job).</summary>
    [HttpPut]
    [Route("bike-sync")]
    public async Task<IActionResult> SetBikeSync([FromQuery] bool enabled)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SetBikeSyncCommand { AthleteId = user.AthleteId, Enabled = enabled });

        return this.ToActionResult(result);
    }

    [HttpGet]
    [Route("activities")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PagedResultViewModel<ActivityViewModel>))]
    public async Task<IActionResult> GetActivities([FromQuery] int page = 0, [FromQuery] int pageSize = 20)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetStravaActivitiesQuery
        {
            AthleteId = user.AthleteId,
            UserId = user.UserId,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result.ToViewModel());
    }

    [HttpGet]
    [Route("activity-sync-history")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<ActivitySyncHistoryViewModel>))]
    public async Task<IActionResult> GetActivitySyncHistory([FromQuery] int take = 20)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var history = await _mediator.Send(new GetActivitySyncHistoryQuery { AthleteId = user.AthleteId, Take = take });

        return Ok(history.ToViewModels());
    }

    private async Task<bool> ScheduleBackfillAsync(int athleteId)
    {
        var schedulerFactory = HttpContext.RequestServices.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        var jobKey = BackfillActivitiesJob.KeyFor(athleteId);

        // Guard against toggle-spam: don't queue a second backfill while one is already scheduled/running.
        if (await scheduler.CheckExists(jobKey))
        {
            return false;
        }

        var job = JobBuilder.Create<BackfillActivitiesJob>()
            .WithIdentity(jobKey)
            .UsingJobData(BackfillActivitiesJob.AthleteIdKey, athleteId)
            .Build();

        var trigger = TriggerBuilder.Create().StartNow().Build();

        await scheduler.ScheduleJob(job, trigger);
        return true;
    }

    [HttpGet]
    [Route("bikes")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<StravaBikeViewModel>))]
    public async Task<IActionResult> GetBikes()
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var bikes = await _mediator.Send(new GetStravaBikesQuery
        {
            AthleteId = user.AthleteId,
            UserId = user.UserId
        });

        return Ok(bikes.ToViewModels());
    }

    [HttpGet]
    [Route("sync-status")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(StravaSyncStatusViewModel))]
    public async Task<IActionResult> GetSyncStatus()
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var status = await _mediator.Send(new GetStravaSyncStatusQuery { AthleteId = user.AthleteId });

        return Ok(status.ToViewModel());
    }

    /// <summary>Link a Strava bike to an existing bt.bike (create the bt.bike_link).</summary>
    [HttpPost]
    [Route("bikes/{gearId}/link")]
    public async Task<IActionResult> LinkBike([FromRoute] string gearId, [FromBody] LinkStravaBikeViewModel model)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new LinkStravaBikeCommand
        {
            BikeId = model.BikeId,
            StravaGearId = gearId,
            UserId = user.UserId,
            AthleteId = user.AthleteId
        });

        return this.ToActionResult(result);
    }

    /// <summary>Remove the link between a Strava bike and a bt.bike (unlink, callable from either side).</summary>
    [HttpDelete]
    [Route("bikes/{gearId}/link")]
    public async Task<IActionResult> UnlinkBike([FromRoute] string gearId)
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new UnlinkStravaBikeCommand
        {
            StravaGearId = gearId,
            UserId = user.UserId
        });

        return this.ToActionResult(result);
    }
}
