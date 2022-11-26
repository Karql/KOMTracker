using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Ranking;
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
    public async Task<IActionResult> Index([FromQuery(Name = "club_id")] long? clubId)
    {
        var ranking = await _mediator.Send(new GetRankingQuery { ClubId = clubId });

        return Ok(_mapper.Map<IEnumerable<AthleteRankingViewModel>>(ranking));
    }
}
