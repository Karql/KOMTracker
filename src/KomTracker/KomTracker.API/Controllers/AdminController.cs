using KomTracker.API.Attributes;
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
    public async Task<ActionResult> TrackKoms()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        await _mediator.Send(new TrackKomsCommand(), cancellationTokenSource.Token);

        return new NoContentResult();
    }
}