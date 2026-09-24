using KomTracker.API.Attributes;
using KomTracker.API.Shared.ViewModels.Suggestions;
using KomTracker.Application.Queries.Component;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("bike-tracker/suggestions")]
[ApiController]
[BearerAuthorize()]
public class SuggestionsController : BaseApiController<SuggestionsController>
{
    /// <summary>Distinct Brand / Model / Purchase place hints across the user's bikes + components (autocomplete).</summary>
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(PurchaseSuggestionsViewModel))]
    public async Task<IActionResult> GetPurchaseSuggestions()
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var suggestions = await _mediator.Send(new GetPurchaseSuggestionsQuery { UserId = userId });

        return Ok(new PurchaseSuggestionsViewModel
        {
            Brands = suggestions.Brands,
            Models = suggestions.Models,
            PurchasePlaces = suggestions.PurchasePlaces
        });
    }
}
