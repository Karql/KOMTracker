using FluentResults;
using FluentValidation;
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using MediatR;

namespace KomTracker.Application.Commands.Expense;

public class ChangeExpenseLifecycleCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public ExpenseLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SalePlace { get; set; }

    /// <summary>Optional note to save alongside the transition (e.g. why it was archived/sold). Null = leave unchanged.</summary>
    public string? Notes { get; set; }
}

public class ChangeExpenseLifecycleCommandValidator : AbstractValidator<ChangeExpenseLifecycleCommand>
{
    public ChangeExpenseLifecycleCommandValidator()
    {
        RuleFor(x => x.Lifecycle).IsInEnum();
        RuleFor(x => x.SalePlace).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);

        When(x => x.Lifecycle == ExpenseLifecycle.Sold, () =>
        {
            RuleFor(x => x.SaleDate).NotNull();
            RuleFor(x => x.SalePrice).NotNull().GreaterThanOrEqualTo(0);
        });
    }
}

public class ChangeExpenseLifecycleCommandHandler : IRequestHandler<ChangeExpenseLifecycleCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public ChangeExpenseLifecycleCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(ChangeExpenseLifecycleCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IExpenseRepository>();
        var expense = await repo.GetExpenseAsync(request.Id);

        if (expense is null)
        {
            return Result.Fail(new NotFoundError($"Expense {request.Id} not found."));
        }

        if (expense.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Expense does not belong to the current user."));
        }

        expense.Lifecycle = request.Lifecycle;

        if (request.Notes is not null)
        {
            expense.Notes = request.Notes;
        }

        if (request.Lifecycle == ExpenseLifecycle.Sold)
        {
            expense.SaleDate = ComponentDateHelper.EnsureUtc(request.SaleDate);
            expense.SalePrice = request.SalePrice;
            expense.SalePlace = request.SalePlace;
        }

        // Leaving Sold keeps the sale details (only shown while Sold), so a re-sale starts pre-filled.

        repo.UpdateExpense(expense);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}
