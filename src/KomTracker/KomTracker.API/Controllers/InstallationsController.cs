using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.Installation;
using KomTracker.Application.Commands.Installation;
using KomTracker.Application.Queries.Installation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("installations")]
[ApiController]
[BearerAuthorize()]
public class InstallationsController : BaseApiController<InstallationsController>
{
    /// <summary>Installations for a bike (?bikeId=) or a component (?componentId=), current first.</summary>
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<InstallationViewModel>))]
    public async Task<IActionResult> GetInstallations([FromQuery] int? bikeId, [FromQuery] int? componentId)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        if (bikeId is int bId)
        {
            var byBike = await _mediator.Send(new GetBikeInstallationsQuery { BikeId = bId, UserId = userId });
            return Ok(byBike.ToViewModels());
        }

        if (componentId is int cId)
        {
            var byComponent = await _mediator.Send(new GetComponentInstallationsQuery { ComponentId = cId, UserId = userId });
            return Ok(byComponent.ToViewModels());
        }

        return BadRequest("Provide either bikeId or componentId.");
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(InstallationViewModel))]
    public async Task<IActionResult> Install([FromBody] InstallComponentViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new InstallComponentCommand
        {
            UserId = userId,
            ComponentId = model.ComponentId,
            BikeId = model.BikeId,
            Type = model.Type,
            DateFrom = model.DateFrom,
            Position = model.Position,
            ManualDistanceKm = model.ManualDistanceKm,
            ManualMovingHours = model.ManualMovingHours,
            ManualElevationM = model.ManualElevationM
        });

        return this.ToActionResult(result, installation => installation.ToViewModel());
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateInstallationViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new UpdateInstallationCommand
        {
            UserId = userId,
            InstallationId = id,
            BikeId = model.BikeId,
            Position = model.Position,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            ManualDistanceKm = model.ManualDistanceKm,
            ManualMovingHours = model.ManualMovingHours,
            ManualElevationM = model.ManualElevationM
        });

        return this.ToActionResult(result);
    }

    [HttpPut]
    [Route("{id}/move")]
    public async Task<IActionResult> Move([FromRoute] int id, [FromBody] MoveInstallationViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new MoveInstallationCommand
        {
            UserId = userId,
            InstallationId = id,
            NewBikeId = model.NewBikeId,
            NewPosition = model.NewPosition,
            MoveDate = model.MoveDate
        });

        return this.ToActionResult(result);
    }

    [HttpPut]
    [Route("{id}/remove")]
    public async Task<IActionResult> Remove([FromRoute] int id, [FromBody] RemoveInstallationViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new RemoveInstallationCommand
        {
            UserId = userId,
            InstallationId = id,
            DateTo = model.DateTo
        });

        return this.ToActionResult(result);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteInstallationCommand { UserId = userId, InstallationId = id });

        return this.ToActionResult(result);
    }
}
