using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.Component;
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Queries.Component;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("components")]
[ApiController]
[BearerAuthorize()]
public class ComponentsController : BaseApiController<ComponentsController>
{
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<ComponentViewModel>))]
    public async Task<IActionResult> GetComponents([FromQuery(Name = "include_inactive")] bool includeInactive = false)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var components = await _mediator.Send(new GetComponentsQuery { UserId = userId, IncludeInactive = includeInactive });

        return Ok(components.ToViewModels());
    }

    [HttpGet]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ComponentViewModel))]
    public async Task<IActionResult> GetComponent([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var component = await _mediator.Send(new GetComponentQuery { Id = id, UserId = userId });

        return component is null ? NotFound() : Ok(component.ToViewModel());
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ComponentViewModel))]
    public Task<IActionResult> AddComponent([FromBody] SaveComponentViewModel model)
        => SaveComponentAsync(null, model);

    [HttpPut]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ComponentViewModel))]
    public Task<IActionResult> UpdateComponent([FromRoute] int id, [FromBody] SaveComponentViewModel model)
        => SaveComponentAsync(id, model);

    [HttpPut]
    [Route("{id}/lifecycle")]
    public async Task<IActionResult> ChangeLifecycle([FromRoute] int id, [FromBody] ChangeComponentLifecycleViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ChangeComponentLifecycleCommand
        {
            Id = id,
            UserId = userId,
            Lifecycle = model.Lifecycle,
            SaleDate = model.SaleDate,
            SalePrice = model.SalePrice,
            Notes = model.Notes
        });

        return this.ToActionResult(result);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteComponent([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteComponentCommand { Id = id, UserId = userId });

        return this.ToActionResult(result);
    }

    private async Task<IActionResult> SaveComponentAsync(int? id, SaveComponentViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveComponentCommand
        {
            Id = id,
            UserId = userId,
            Name = model.Name,
            Brand = model.Brand,
            Model = model.Model,
            Category = model.Category,
            WeightKg = model.WeightKg,
            Notes = model.Notes,
            Price = model.Price,
            PurchasePlace = model.PurchasePlace,
            PurchaseDate = model.PurchaseDate,
            InitialDistanceKm = model.InitialDistanceKm,
            InitialMovingHours = model.InitialMovingHours,
            InitialElevationM = model.InitialElevationM,
            WarehouseId = model.WarehouseId
        });

        return this.ToActionResult(result, component => component.ToViewModel());
    }
}
