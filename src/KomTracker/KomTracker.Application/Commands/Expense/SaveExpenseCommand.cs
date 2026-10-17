using FluentResults;
using FluentValidation;
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using MediatR;

namespace KomTracker.Application.Commands.Expense;

/// <summary>Creates an expense when <see cref="Id"/> is null, otherwise updates the existing one.</summary>
public class SaveExpenseCommand : IRequest<Result<ExpenseEntity>>
{
    // Server-owned (set by the controller from route/claims), not part of the request body.
    public int? Id { get; set; }
    public string UserId { get; set; } = default!;

    // Editable fields — mirror SaveExpenseViewModel (parity test guards drift).
    public string Name { get; set; } = default!;
    public ExpenseCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PurchasePlace { get; set; }
    public string? Notes { get; set; }

    // Sale details — applied only while the expense is Sold (fix a forgotten/wrong sale). Ignored otherwise.
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SalePlace { get; set; }
}

public class SaveExpenseCommandValidator : AbstractValidator<SaveExpenseCommand>
{
    public SaveExpenseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PurchaseDate).NotEmpty();
        RuleFor(x => x.PurchasePlace).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).When(x => x.SalePrice.HasValue);
        RuleFor(x => x.SalePlace).MaximumLength(200);
    }
}

public class SaveExpenseCommandHandler : IRequestHandler<SaveExpenseCommand, Result<ExpenseEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SaveExpenseCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<ExpenseEntity>> Handle(SaveExpenseCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IExpenseRepository>();

        ExpenseEntity expense;

        if (request.Id is null)
        {
            expense = new ExpenseEntity { UserId = request.UserId, Lifecycle = ExpenseLifecycle.Active };
            Apply(request, expense);
            repo.AddExpense(expense);
        }
        else
        {
            var existing = await repo.GetExpenseAsync(request.Id.Value);

            if (existing is null)
            {
                return Result.Fail(new NotFoundError($"Expense {request.Id} not found."));
            }

            if (existing.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Expense does not belong to the current user."));
            }

            Apply(request, existing);

            // Sale details are editable only while Sold (corrections); the sell flow owns the Active → Sold transition.
            if (existing.Lifecycle == ExpenseLifecycle.Sold)
            {
                existing.SaleDate = ComponentDateHelper.EnsureUtc(request.SaleDate);
                existing.SalePrice = request.SalePrice;
                existing.SalePlace = request.SalePlace;
            }

            repo.UpdateExpense(existing);
            expense = existing;
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok(expense);
    }

    private static void Apply(SaveExpenseCommand request, ExpenseEntity expense)
    {
        expense.Name = request.Name;
        expense.Category = request.Category;
        expense.Price = request.Price;
        expense.PurchaseDate = DateTime.SpecifyKind(request.PurchaseDate, DateTimeKind.Utc);
        expense.PurchasePlace = request.PurchasePlace;
        expense.Notes = request.Notes;
    }
}
