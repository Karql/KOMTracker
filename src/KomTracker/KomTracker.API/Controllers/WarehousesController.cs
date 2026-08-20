using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.Warehouse;
using KomTracker.Application.Commands.Warehouse;
using KomTracker.Application.Queries.Warehouse;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("warehouses")]
[ApiController]
[BearerAuthorize()]
public class WarehousesController : BaseApiController<WarehousesController>
{
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<WarehouseViewModel>))]
    public async Task<IActionResult> GetWarehouses()
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var warehouses = await _mediator.Send(new GetWarehousesQuery { UserId = userId });

        return Ok(warehouses.ToViewModels());
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(WarehouseViewModel))]
    public Task<IActionResult> AddWarehouse([FromBody] SaveWarehouseViewModel model)
        => SaveWarehouseAsync(null, model);

    [HttpPut]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(WarehouseViewModel))]
    public Task<IActionResult> UpdateWarehouse([FromRoute] int id, [FromBody] SaveWarehouseViewModel model)
        => SaveWarehouseAsync(id, model);

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteWarehouse([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteWarehouseCommand { Id = id, UserId = userId });

        return this.ToActionResult(result);
    }

    private async Task<IActionResult> SaveWarehouseAsync(int? id, SaveWarehouseViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveWarehouseCommand
        {
            Id = id,
            UserId = userId,
            Name = model.Name
        });

        return this.ToActionResult(result, warehouse => warehouse.ToViewModel());
    }
}
