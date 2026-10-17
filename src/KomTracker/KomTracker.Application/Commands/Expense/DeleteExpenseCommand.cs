using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Expense;

public class DeleteExpenseCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public DeleteExpenseCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
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

        repo.DeleteExpense(expense);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}
