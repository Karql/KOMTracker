#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFExpenseRepository : EFBaseRepository, IExpenseRepository
{
    public async Task<IEnumerable<ExpenseEntity>> GetExpensesAsync(string userId)
    {
        return await _context.Expense
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.PurchaseDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<ExpenseEntity?> GetExpenseAsync(int id)
    {
        return await _context.Expense.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void AddExpense(ExpenseEntity expense)
    {
        _context.Expense.Add(expense);
    }

    public void UpdateExpense(ExpenseEntity expense)
    {
        _context.Expense.Update(expense);
    }

    public void DeleteExpense(ExpenseEntity expense)
    {
        _context.Expense.Remove(expense);
    }

    public async Task<(IReadOnlyList<string> PurchasePlaces, IReadOnlyList<string> SalePlaces)> GetDistinctPlacesAsync(string userId)
    {
        var scoped = _context.Expense.AsNoTracking().Where(x => x.UserId == userId);

        var places = await scoped.Where(x => x.PurchasePlace != null && x.PurchasePlace != "").Select(x => x.PurchasePlace!).Distinct().ToListAsync();
        var salePlaces = await scoped.Where(x => x.SalePlace != null && x.SalePlace != "").Select(x => x.SalePlace!).Distinct().ToListAsync();

        return (places, salePlaces);
    }
}
