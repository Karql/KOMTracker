using KomTracker.API.Attributes;
using KomTracker.API.Extensions;
using KomTracker.API.Shared.ViewModels.Expense;
using KomTracker.Application.Commands.Expense;
using KomTracker.Application.Queries.Expense;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KomTracker.API.Controllers;

[Route("expenses")]
[ApiController]
[BearerAuthorize()]
public class ExpensesController : BaseApiController<ExpensesController>
{
    [HttpGet]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IEnumerable<ExpenseViewModel>))]
    public async Task<IActionResult> GetExpenses()
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var expenses = await _mediator.Send(new GetExpensesQuery { UserId = userId });

        return Ok(expenses.ToViewModels());
    }

    [HttpGet]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ExpenseViewModel))]
    public async Task<IActionResult> GetExpense([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var expense = await _mediator.Send(new GetExpenseQuery { Id = id, UserId = userId });

        return expense is null ? NotFound() : Ok(expense.ToViewModel());
    }

    [HttpPost]
    [Route("")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ExpenseViewModel))]
    public Task<IActionResult> AddExpense([FromBody] SaveExpenseViewModel model)
        => SaveExpenseAsync(null, model);

    [HttpPut]
    [Route("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ExpenseViewModel))]
    public Task<IActionResult> UpdateExpense([FromRoute] int id, [FromBody] SaveExpenseViewModel model)
        => SaveExpenseAsync(id, model);

    [HttpPut]
    [Route("{id}/lifecycle")]
    public async Task<IActionResult> ChangeLifecycle([FromRoute] int id, [FromBody] ChangeExpenseLifecycleViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ChangeExpenseLifecycleCommand
        {
            Id = id,
            UserId = userId,
            Lifecycle = model.Lifecycle,
            SaleDate = model.SaleDate,
            SalePrice = model.SalePrice,
            SalePlace = model.SalePlace,
            Notes = model.Notes
        });

        return this.ToActionResult(result);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteExpense([FromRoute] int id)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new DeleteExpenseCommand { Id = id, UserId = userId });

        return this.ToActionResult(result);
    }

    private async Task<IActionResult> SaveExpenseAsync(int? id, SaveExpenseViewModel model)
    {
        var userId = GetCurrentUser()?.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        // Map EVERY view-model field (incl. the sale ones) — a missed field silently resets on save.
        var result = await _mediator.Send(new SaveExpenseCommand
        {
            Id = id,
            UserId = userId,
            Name = model.Name,
            Category = model.Category,
            Price = model.Price,
            PurchaseDate = model.PurchaseDate,
            PurchasePlace = model.PurchasePlace,
            Notes = model.Notes,
            SaleDate = model.SaleDate,
            SalePrice = model.SalePrice,
            SalePlace = model.SalePlace
        });

        return this.ToActionResult(result, expense => expense.ToViewModel());
    }
}
