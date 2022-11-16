using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Queries.Stats;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("stats")]
[ApiController]
[BearerAuthorize()]
public class StatsController : BaseApiController<StatsController>
{
    [HttpGet]
    [Route("koms-changes")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<LastKomsChangesViewModel>))]
    public async Task<IActionResult> GetLastKomsChangesAsync([FromQuery(Name = "club_id")] long? clubId)
    {
        var lastChanges = await _mediator.Send(new GetLastKomsChangesQuery { ClubId = clubId });

        return Ok(_mapper.Map<IEnumerable<LastKomsChangesViewModel>>(lastChanges));
    }
}
