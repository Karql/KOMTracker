using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.ServiceRecord;
using KomTracker.Application.Commands.ServiceRecord;
using KomTracker.Application.Queries.ServiceRecord;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("services")]
[ApiController]
[BearerAuthorize()]
public class ServicesController : BaseApiController<ServicesController>
{
    /// <summary>The caller's services (newest first); <c>bikeId</c> / <c>componentId</c> narrow to those covering it.</summary>
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<ServiceRecordViewModel>))]
    public async Task<IActionResult> GetServices([FromQuery] int? bikeId, [FromQuery] int? componentId)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var services = await _mediator.Send(new GetServiceRecordsQuery { UserId = userId, BikeId = bikeId, ComponentId = componentId });

        return Ok(services.ToViewModels());
    }

    [HttpGet]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ServiceRecordViewModel))]
    public async Task<IActionResult> GetService([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var service = await _mediator.Send(new GetServiceRecordQuery { Id = id, UserId = userId });

        return service is null ? NotFound() : Ok(service.ToViewModel());
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ServiceRecordViewModel))]
    public Task<IActionResult> AddService([FromBody] SaveServiceRecordViewModel model)
        => SaveServiceAsync(null, model);

    [HttpPut]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ServiceRecordViewModel))]
    public Task<IActionResult> UpdateService([FromRoute] int id, [FromBody] SaveServiceRecordViewModel model)
        => SaveServiceAsync(id, model);

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteService([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteServiceRecordCommand { Id = id, UserId = userId });

        return this.ToActionResult(result);
    }

    private async Task<IActionResult> SaveServiceAsync(int? id, SaveServiceRecordViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        // Map EVERY view-model field — a missed field silently resets on save.
        var result = await _mediator.Send(new SaveServiceRecordCommand
        {
            Id = id,
            UserId = userId,
            Name = model.Name,
            Date = model.Date,
            Place = model.Place,
            Cost = model.Cost,
            Notes = model.Notes,
            Targets = model.Targets
        });

        if (result.IsFailed)
        {
            return this.ToActionResult(result.ToResult());
        }

        // Re-read so the response carries the target names and the freshly computed mileage.
        var saved = await _mediator.Send(new GetServiceRecordQuery { Id = result.Value.Id, UserId = userId });

        return Ok(saved!.ToViewModel());
    }
}
