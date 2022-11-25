using KomTracker.API.Attributes;
using KomTracker.Application.Commands.Club;
using KomTracker.Application.Commands.Stats;
using KomTracker.Application.Commands.Tracking;
using Microsoft.AspNetCore.Mvc;
using static KomTracker.Application.Constants;

namespace KomTracker.API.Controllers;


[Route("admin")]
[ApiController]
[BearerAuthorize(Roles = Roles.Admin)]
public class AdminController : BaseApiController<AdminController>
{
    private readonly IServiceProvider _serviceProvider;

    public AdminController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    [HttpPut("track-koms")]
    public async Task<ActionResult> TrackKoms(CancellationToken cancellationToken)
    {
        await _mediator.Send(new TrackKomsCommand(), cancellationToken);

        return new NoContentResult();
    }

    [HttpPut("refresh-segments")]
    public async Task<ActionResult> RefreshSegments(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RefreshSegmentsCommand(), cancellationToken);

        return new NoContentResult();
    }

    [HttpPut("refresh-clubs")]
    public async Task<ActionResult> RefreshClubs(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RefreshClubsCommand(), cancellationToken);

        return new NoContentResult();
    }

    [HttpPut("refresh-stats")]
    public async Task<ActionResult> RefreshStats(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RefreshStatsCommand(), cancellationToken);

        return new NoContentResult();
    }
}