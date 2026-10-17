using KomTracker.Domain.Entities.Expense;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IExpenseRepository : IRepository
{
    /// <summary>All of the user's expenses (any lifecycle), newest first.</summary>
    Task<IEnumerable<ExpenseEntity>> GetExpensesAsync(string userId);
    Task<ExpenseEntity?> GetExpenseAsync(int id);
    void AddExpense(ExpenseEntity expense);
    void UpdateExpense(ExpenseEntity expense);
    void DeleteExpense(ExpenseEntity expense);

    /// <summary>DB-side DISTINCT of the non-empty purchase / sale place values for the user's expenses (autocomplete hints).</summary>
    Task<(IReadOnlyList<string> PurchasePlaces, IReadOnlyList<string> SalePlaces)> GetDistinctPlacesAsync(string userId);
}
