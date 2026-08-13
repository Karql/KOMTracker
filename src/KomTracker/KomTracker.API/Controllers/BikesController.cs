using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.Application.Commands.Bike;
using KomTracker.Application.Queries.Bike;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("bikes")]
[ApiController]
[BearerAuthorize()]
public class BikesController : BaseApiController<BikesController>
{
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<BikeViewModel>))]
    public async Task<IActionResult> GetBikes([FromQuery(Name = "include_inactive")] bool includeInactive = false)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var bikes = await _mediator.Send(new GetBikesQuery { UserId = userId, IncludeInactive = includeInactive });

        return Ok(bikes.ToViewModels());
    }

    [HttpGet]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(BikeViewModel))]
    public async Task<IActionResult> GetBike([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var bike = await _mediator.Send(new GetBikeQuery { Id = id, UserId = userId });

        return bike is null ? NotFound() : Ok(bike.ToViewModel());
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(BikeViewModel))]
    public Task<IActionResult> AddBike([FromBody] SaveBikeViewModel model)
        => SaveBikeAsync(null, model);

    [HttpPut]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(BikeViewModel))]
    public Task<IActionResult> UpdateBike([FromRoute] int id, [FromBody] SaveBikeViewModel model)
        => SaveBikeAsync(id, model);

    [HttpPut]
    [Route("{id}/lifecycle")]
    public async Task<IActionResult> ChangeLifecycle([FromRoute] int id, [FromBody] ChangeBikeLifecycleViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ChangeBikeLifecycleCommand
        {
            Id = id,
            UserId = userId,
            Lifecycle = model.Lifecycle,
            SaleDate = model.SaleDate,
            SalePrice = model.SalePrice
        });

        return this.ToActionResult(result);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteBike([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteBikeCommand { Id = id, UserId = userId });

        return this.ToActionResult(result);
    }

    private async Task<IActionResult> SaveBikeAsync(int? id, SaveBikeViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new SaveBikeCommand
        {
            Id = id,
            UserId = userId,
            Name = model.Name,
            Brand = model.Brand,
            Model = model.Model,
            Type = model.Type,
            WeightKg = model.WeightKg,
            Notes = model.Notes,
            Price = model.Price,
            PurchasePlace = model.PurchasePlace,
            PurchaseDate = model.PurchaseDate,
            InitialDistanceKm = model.InitialDistanceKm,
            InitialMovingHours = model.InitialMovingHours,
            InitialElevationM = model.InitialElevationM,
            StravaGearId = model.StravaGearId
        });

        return this.ToActionResult(result, bike => bike.ToViewModel());
    }
}
