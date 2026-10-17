#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Expense;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Expense;
using NSubstitute;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Expense;

public class ExpenseCommandHandlersTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _expenseRepo = Substitute.For<IExpenseRepository>();
        _komUoW.GetRepository<IExpenseRepository>().Returns(_expenseRepo);
    }

    private static SaveExpenseCommand Save(int? id = null) => new()
    {
        Id = id,
        UserId = "u1",
        Name = "Bidon",
        Category = ExpenseCategory.Bottles,
        Price = 38,
        PurchaseDate = new DateTime(2026, 5, 1),
        PurchasePlace = "Biostern"
    };

    [Fact]
    public async Task Create_adds_an_active_expense_with_a_utc_date()
    {
        var res = await new SaveExpenseCommandHandler(_komUoW).Handle(Save(), CancellationToken.None);

        res.Should().BeSuccess();
        _expenseRepo.Received().AddExpense(Arg.Is<ExpenseEntity>(e =>
            e.UserId == "u1" && e.Name == "Bidon" && e.Category == ExpenseCategory.Bottles && e.Price == 38
            && e.Lifecycle == ExpenseLifecycle.Active && e.PurchaseDate.Kind == DateTimeKind.Utc && e.PurchasePlace == "Biostern"));
    }

    [Fact]
    public async Task Update_of_another_users_expense_is_forbidden()
    {
        _expenseRepo.GetExpenseAsync(3).Returns(new ExpenseEntity { Id = 3, UserId = "other", Name = "x" });

        var res = await new SaveExpenseCommandHandler(_komUoW).Handle(Save(3), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>().Should().BeTrue();
        _expenseRepo.DidNotReceive().UpdateExpense(Arg.Any<ExpenseEntity>());
    }

    [Fact]
    public async Task Update_of_a_missing_expense_is_not_found()
    {
        _expenseRepo.GetExpenseAsync(3).Returns((ExpenseEntity?)null);

        var res = await new SaveExpenseCommandHandler(_komUoW).Handle(Save(3), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>().Should().BeTrue();
    }

    [Fact]
    public async Task Update_applies_sale_details_only_while_sold()
    {
        var sold = new ExpenseEntity { Id = 3, UserId = "u1", Name = "x", Lifecycle = ExpenseLifecycle.Sold };
        _expenseRepo.GetExpenseAsync(3).Returns(sold);
        var cmd = Save(3);
        cmd.SaleDate = new DateTime(2026, 6, 1);
        cmd.SalePrice = 20;
        cmd.SalePlace = "Rafał";

        await new SaveExpenseCommandHandler(_komUoW).Handle(cmd, CancellationToken.None);

        sold.SalePrice.Should().Be(20);
        sold.SalePlace.Should().Be("Rafał");
        sold.SaleDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Update_ignores_sale_details_when_not_sold()
    {
        var active = new ExpenseEntity { Id = 3, UserId = "u1", Name = "x", Lifecycle = ExpenseLifecycle.Active };
        _expenseRepo.GetExpenseAsync(3).Returns(active);
        var cmd = Save(3);
        cmd.SalePrice = 20;

        await new SaveExpenseCommandHandler(_komUoW).Handle(cmd, CancellationToken.None);

        active.SalePrice.Should().BeNull();
    }

    [Fact]
    public async Task Selling_sets_the_sale_details_and_notes()
    {
        var expense = new ExpenseEntity { Id = 3, UserId = "u1", Name = "Milkit" };
        _expenseRepo.GetExpenseAsync(3).Returns(expense);

        var res = await new ChangeExpenseLifecycleCommandHandler(_komUoW).Handle(new ChangeExpenseLifecycleCommand
        {
            Id = 3, UserId = "u1", Lifecycle = ExpenseLifecycle.Sold,
            SaleDate = new DateTime(2026, 6, 1), SalePrice = 150, SalePlace = "Rafał", Notes = "Sprzedany"
        }, CancellationToken.None);

        res.Should().BeSuccess();
        expense.Lifecycle.Should().Be(ExpenseLifecycle.Sold);
        expense.SalePrice.Should().Be(150);
        expense.SalePlace.Should().Be("Rafał");
        expense.Notes.Should().Be("Sprzedany");
    }

    [Fact]
    public async Task Leaving_sold_keeps_the_sale_details()
    {
        var expense = new ExpenseEntity
        {
            Id = 3, UserId = "u1", Name = "Milkit", Lifecycle = ExpenseLifecycle.Sold,
            SaleDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), SalePrice = 150, SalePlace = "Rafał", Notes = "n"
        };
        _expenseRepo.GetExpenseAsync(3).Returns(expense);

        await new ChangeExpenseLifecycleCommandHandler(_komUoW).Handle(new ChangeExpenseLifecycleCommand
        {
            Id = 3, UserId = "u1", Lifecycle = ExpenseLifecycle.Active
        }, CancellationToken.None);

        expense.Lifecycle.Should().Be(ExpenseLifecycle.Active);
        expense.SalePrice.Should().Be(150);
        expense.SalePlace.Should().Be("Rafał");
        expense.Notes.Should().Be("n"); // null notes = leave unchanged
    }

    [Fact]
    public async Task Lifecycle_change_of_another_users_expense_is_forbidden()
    {
        _expenseRepo.GetExpenseAsync(3).Returns(new ExpenseEntity { Id = 3, UserId = "other", Name = "x" });

        var res = await new ChangeExpenseLifecycleCommandHandler(_komUoW).Handle(new ChangeExpenseLifecycleCommand
        {
            Id = 3, UserId = "u1", Lifecycle = ExpenseLifecycle.Archived
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>().Should().BeTrue();
    }

    [Fact]
    public async Task Delete_removes_own_expense()
    {
        var expense = new ExpenseEntity { Id = 3, UserId = "u1", Name = "x" };
        _expenseRepo.GetExpenseAsync(3).Returns(expense);

        var res = await new DeleteExpenseCommandHandler(_komUoW).Handle(new DeleteExpenseCommand { Id = 3, UserId = "u1" }, CancellationToken.None);

        res.Should().BeSuccess();
        _expenseRepo.Received().DeleteExpense(expense);
    }

    [Fact]
    public async Task Delete_of_a_missing_expense_is_not_found()
    {
        _expenseRepo.GetExpenseAsync(3).Returns((ExpenseEntity?)null);

        var res = await new DeleteExpenseCommandHandler(_komUoW).Handle(new DeleteExpenseCommand { Id = 3, UserId = "u1" }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>().Should().BeTrue();
    }
}
