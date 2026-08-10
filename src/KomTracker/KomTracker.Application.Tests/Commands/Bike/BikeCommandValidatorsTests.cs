using FluentValidation.TestHelper;
using KomTracker.Application.Commands.Bike;
using KomTracker.Domain.Entities.Bike;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Bike;

public class SaveBikeCommandValidatorTests
{
    private readonly SaveBikeCommandValidator _validator = new();

    [Fact]
    public void Fails_when_name_is_empty()
    {
        var result = _validator.TestValidate(new SaveBikeCommand { Name = "", Type = BikeType.Road });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Fails_when_weight_is_negative()
    {
        var result = _validator.TestValidate(new SaveBikeCommand { Name = "Canyon", Type = BikeType.Road, WeightKg = -1 });
        result.ShouldHaveValidationErrorFor(x => x.WeightKg);
    }

    [Fact]
    public void Passes_for_a_valid_command()
    {
        var result = _validator.TestValidate(new SaveBikeCommand
        {
            Name = "Canyon",
            Type = BikeType.Road,
            WeightKg = 7.5m,
            InitialDistanceKm = 0
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class ChangeBikeLifecycleCommandValidatorTests
{
    private readonly ChangeBikeLifecycleCommandValidator _validator = new();

    [Fact]
    public void Sold_requires_sale_date_and_price()
    {
        var result = _validator.TestValidate(new ChangeBikeLifecycleCommand { Lifecycle = BikeLifecycle.Sold });

        result.ShouldHaveValidationErrorFor(x => x.SaleDate);
        result.ShouldHaveValidationErrorFor(x => x.SalePrice);
    }

    [Fact]
    public void Archived_is_valid_without_sale_details()
    {
        var result = _validator.TestValidate(new ChangeBikeLifecycleCommand { Lifecycle = BikeLifecycle.Archived });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
