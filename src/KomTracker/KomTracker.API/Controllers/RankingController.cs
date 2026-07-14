using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Ranking;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Queries.Ranking;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("ranking")]
[ApiController]
[BearerAuthorize()]
public class RankingController : BaseApiController<AthletesController>
{
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<AthleteRankingViewModel>))]
    public async Task<IActionResult> Index([FromQuery(Name = "club_id")] long? clubId, [FromQuery(Name = "activity_type")]string? activityType)
    {
        var ranking = await _mediator.Send(new GetRankingQuery { ClubId = clubId, ActivityType = activityType });

        return Ok(_mapper.Map<IEnumerable<AthleteRankingViewModel>>(ranking));
    }

    /// <summary>The New/Lost KOMs behind one "Koms changes" cell (Ranking details modal).</summary>
    [HttpGet]
    [Route("koms-changes-details")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<EffortViewModel>))]
    public async Task<IActionResult> KomsChangesDetails(
        [FromQuery(Name = "athlete_id")] int athleteId,
        [FromQuery(Name = "period")] KomsChangesPeriod period,
        [FromQuery(Name = "direction")] KomsChangeDirection direction,
        [FromQuery(Name = "activity_type")] string? activityType)
    {
        var koms = await _mediator.Send(new GetKomsChangesDetailsQuery
        {
            AthleteId = athleteId,
            Period = period,
            Direction = direction,
            ActivityType = activityType
        });

        return Ok(_mapper.Map<IEnumerable<EffortViewModel>>(koms));
    }
}
