using FluentValidation.TestHelper;
using KomTracker.Application.Commands.Component;
using KomTracker.Domain.Entities.Component;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Component;

public class SaveComponentCommandValidatorTests
{
    private readonly SaveComponentCommandValidator _validator = new();

    [Fact]
    public void Fails_when_name_is_empty()
    {
        var result = _validator.TestValidate(new SaveComponentCommand { Name = "", Category = ComponentCategory.Chain });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Fails_when_weight_is_negative()
    {
        var result = _validator.TestValidate(new SaveComponentCommand { Name = "Chain", Category = ComponentCategory.Chain, WeightKg = -1 });
        result.ShouldHaveValidationErrorFor(x => x.WeightKg);
    }

    [Fact]
    public void Passes_for_a_valid_command()
    {
        var result = _validator.TestValidate(new SaveComponentCommand
        {
            Name = "SRAM chain",
            Category = ComponentCategory.Chain,
            WeightKg = 0.25m,
            InitialDistanceKm = 0
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class ChangeComponentLifecycleCommandValidatorTests
{
    private readonly ChangeComponentLifecycleCommandValidator _validator = new();

    [Fact]
    public void Sold_requires_sale_date_and_price()
    {
        var result = _validator.TestValidate(new ChangeComponentLifecycleCommand { Lifecycle = ComponentLifecycle.Sold });

        result.ShouldHaveValidationErrorFor(x => x.SaleDate);
        result.ShouldHaveValidationErrorFor(x => x.SalePrice);
    }

    [Fact]
    public void Archived_is_valid_without_sale_details()
    {
        var result = _validator.TestValidate(new ChangeComponentLifecycleCommand { Lifecycle = ComponentLifecycle.Archived });
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class SaveWarehouseCommandValidatorTests
{
    private readonly KomTracker.Application.Commands.Warehouse.SaveWarehouseCommandValidator _validator = new();

    [Fact]
    public void Fails_when_name_is_empty()
    {
        var result = _validator.TestValidate(new KomTracker.Application.Commands.Warehouse.SaveWarehouseCommand { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Passes_for_a_valid_command()
    {
        var result = _validator.TestValidate(new KomTracker.Application.Commands.Warehouse.SaveWarehouseCommand { Name = "Garage" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
