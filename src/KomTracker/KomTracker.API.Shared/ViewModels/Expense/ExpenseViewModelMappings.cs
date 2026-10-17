using System.Collections.Generic;
using System.Linq;
using KomTracker.Domain.Entities.Expense;

namespace KomTracker.API.Shared.ViewModels.Expense;

/// <summary>Explicit entity → view-model mapping (no AutoMapper — compile-time safe).</summary>
public static class ExpenseViewModelMappings
{
    public static ExpenseViewModel ToViewModel(this ExpenseEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Category = e.Category,
        CategoryGroup = ExpenseCategoryMetadata.Group(e.Category),
        Price = e.Price,
        PurchaseDate = e.PurchaseDate,
        PurchasePlace = e.PurchasePlace,
        Notes = e.Notes,
        Lifecycle = e.Lifecycle,
        SaleDate = e.SaleDate,
        SalePrice = e.SalePrice,
        SalePlace = e.SalePlace
    };

    public static IEnumerable<ExpenseViewModel> ToViewModels(this IEnumerable<ExpenseEntity> expenses)
        => expenses.Select(ToViewModel);
}
