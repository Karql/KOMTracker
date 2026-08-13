using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.BikeTracker;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Queries.Strava;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("bike-tracker/strava")]
[ApiController]
[BearerAuthorize()]
public class StravaBikesController : BaseApiController<StravaBikesController>
{
    /// <summary>The single "Sync from Strava" action: sync gear (strava.bike) + enable activity sync + backfill.</summary>
    [HttpPost]
    [Route("sync")]
    public async Task<IActionResult> Sync()
    {
        var user = GetCurrentUser();
        if (user?.UserId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ActivateStravaSyncCommand
        {
            AthleteId = user.AthleteId,
            UserId = user.UserId
        });

        return this.ToActionResult(result);
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
