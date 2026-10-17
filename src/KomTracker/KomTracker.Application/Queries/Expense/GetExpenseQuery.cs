using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using MediatR;

namespace KomTracker.Application.Queries.Expense;

/// <summary>A single expense of the caller; null when missing or owned by someone else (looks like "not found").</summary>
public class GetExpenseQuery : IRequest<ExpenseEntity?>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetExpenseQueryHandler : IRequestHandler<GetExpenseQuery, ExpenseEntity?>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetExpenseQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<ExpenseEntity?> Handle(GetExpenseQuery request, CancellationToken cancellationToken)
    {
        var expense = await _komUoW.GetRepository<IExpenseRepository>().GetExpenseAsync(request.Id);

        return expense is null || expense.UserId != request.UserId ? null : expense;
    }
}
