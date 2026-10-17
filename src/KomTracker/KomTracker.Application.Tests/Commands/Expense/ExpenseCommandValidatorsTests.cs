using System;
using FluentValidation.TestHelper;
using KomTracker.Application.Commands.Expense;
using KomTracker.Domain.Entities.Expense;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Expense;

public class ExpenseCommandValidatorsTests
{
    private readonly SaveExpenseCommandValidator _save = new();
    private readonly ChangeExpenseLifecycleCommandValidator _lifecycle = new();

    private static SaveExpenseCommand Valid() => new()
    {
        Name = "Łatki", Category = ExpenseCategory.Tubes, Price = 9.99m, PurchaseDate = new DateTime(2026, 5, 1)
    };

    [Fact]
    public void Save_passes_for_a_valid_command()
        => _save.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Save_allows_a_zero_amount()
    {
        var cmd = Valid();
        cmd.Price = 0;
        _save.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Save_fails_without_name_date_or_with_a_negative_amount()
    {
        var cmd = Valid();
        cmd.Name = "";
        cmd.PurchaseDate = default;
        cmd.Price = -1;

        var result = _save.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDate);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Selling_requires_a_sale_date_and_price()
    {
        var result = _lifecycle.TestValidate(new ChangeExpenseLifecycleCommand { Lifecycle = ExpenseLifecycle.Sold });

        result.ShouldHaveValidationErrorFor(x => x.SaleDate);
        result.ShouldHaveValidationErrorFor(x => x.SalePrice);
    }

    [Fact]
    public void Archiving_needs_no_sale_data()
        => _lifecycle.TestValidate(new ChangeExpenseLifecycleCommand { Lifecycle = ExpenseLifecycle.Archived })
            .ShouldNotHaveAnyValidationErrors();
}
