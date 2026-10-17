using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using MediatR;

namespace KomTracker.Application.Queries.Expense;

/// <summary>All of the user's expenses (any lifecycle — the list filters client-side), newest first.</summary>
public class GetExpensesQuery : IRequest<IEnumerable<ExpenseEntity>>
{
    public string UserId { get; set; } = default!;
}

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, IEnumerable<ExpenseEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetExpensesQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public Task<IEnumerable<ExpenseEntity>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
        => _komUoW.GetRepository<IExpenseRepository>().GetExpensesAsync(request.UserId);
}
