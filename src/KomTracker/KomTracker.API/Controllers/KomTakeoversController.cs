using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.KomTakeover;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Queries.KomTakeover;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("kom-takeovers")]
[ApiController]
[BearerAuthorize()]
public class KomTakeoversController : BaseApiController<KomTakeoversController>
{
    /// <summary>Head-to-head takeover pairs (winner on the left) for the Battle Field grid.</summary>
    [HttpGet]
    [Route("pairs")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<KomTakeoverPairViewModel>))]
    public async Task<IActionResult> Pairs(
        [FromQuery(Name = "date_from")] DateTime? dateFrom,
        [FromQuery(Name = "date_to")] DateTime? dateTo,
        [FromQuery(Name = "activity_type")] string? activityType,
        [FromQuery(Name = "club_id")] long? clubId)
    {
        var pairs = await _mediator.Send(new GetKomTakeoverPairsQuery
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            ActivityType = activityType,
            ClubId = clubId
        });

        return Ok(_mapper.Map<IEnumerable<KomTakeoverPairViewModel>>(pairs));
    }

    /// <summary>Individual taken efforts behind a single direction of a pair (Battle Field details modal).</summary>
    [HttpGet]
    [Route("efforts")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<EffortViewModel>))]
    public async Task<IActionResult> Efforts(
        [FromQuery(Name = "winner_athlete_id")] int winnerAthleteId,
        [FromQuery(Name = "loser_athlete_id")] int loserAthleteId,
        [FromQuery(Name = "date_from")] DateTime? dateFrom,
        [FromQuery(Name = "date_to")] DateTime? dateTo,
        [FromQuery(Name = "activity_type")] string? activityType)
    {
        var efforts = await _mediator.Send(new GetKomTakeoverEffortsQuery
        {
            WinnerAthleteId = winnerAthleteId,
            LoserAthleteId = loserAthleteId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            ActivityType = activityType
        });

        return Ok(_mapper.Map<IEnumerable<EffortViewModel>>(efforts));
    }
}
