using KomTracker.API.Attributes;
using KomTracker.Application.Queries.CostAnalysis;
using KomTracker.Domain.Entities.CostAnalysis;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("cost-analysis")]
[ApiController]
[BearerAuthorize()]
public class CostAnalysisController : BaseApiController<CostAnalysisController>
{
    /// <summary>The caller's attributed cost ledger + per-bike km (the page slices it client-side, D-COST-1).</summary>
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(CostAnalysisReport))]
    public async Task<IActionResult> GetCostAnalysis()
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _mediator.Send(new GetCostAnalysisQuery { UserId = userId }));
    }
}
