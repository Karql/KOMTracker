using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Queries.Athlete;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;
[Route("athletes")]
[ApiController]
[BearerAuthorize()]
public class AthletesController : BaseApiController<AthletesController>
{ 
    [HttpGet]
    [Route("{athleteId}/koms")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<EffortViewModel>))]
    public async Task<IActionResult> GetAllKomsAsync([FromRoute]int athleteId)
    {
        var koms = await _mediator.Send(new GetAllKomsQuery { AthleteId = athleteId });

        return Ok(_mapper.Map<IEnumerable<EffortViewModel>>(koms));
    }
}
