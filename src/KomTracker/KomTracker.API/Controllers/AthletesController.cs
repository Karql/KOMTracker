using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Commands.Account;
using KomTracker.Application.Queries.Athlete;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

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

    [HttpGet]
    [Route("{athleteId}/koms-changes")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<EffortViewModel>))]
    public async Task<IActionResult> GetLastKomsChangesAsync(int athleteId)
    {
        var koms = await _mediator.Send(new GetLastKomsChangesQuery { AthleteId = athleteId });

        return Ok(_mapper.Map<IEnumerable<EffortViewModel>>(koms));
    }

    [HttpGet]
    [Route("{athleteId}/summaries")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<KomsSummaryViewModel>))]
    public async Task<IActionResult> GetSummaries([FromRoute]int athleteId)
    {
        var komsSummaries = await _mediator.Send(new GetKomsSummariesQuery { AthleteId = athleteId });

        return Ok(_mapper.Map<IEnumerable<KomsSummaryViewModel>>(komsSummaries));
    }

    [HttpPut]
    [Route("{athleteId}/change-email/{newEmail}")]
    public async Task<IActionResult> ChangeEmail([FromRoute][Required]int athleteId, [FromRoute][Required]string newEmail)
    {
        var user = GetCurrentUser();

        if (user?.AthleteId != athleteId)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        await _mediator.Send(new ChangeEmailCommand { AthleteId = athleteId, NewEmail = newEmail });

        return Ok();
    }
}
