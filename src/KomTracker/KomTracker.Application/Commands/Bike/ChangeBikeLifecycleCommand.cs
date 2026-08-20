using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Commands.Bike;

public class ChangeBikeLifecycleCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public BikeLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }

    /// <summary>Optional note to save alongside the transition (e.g. why it was archived/sold). Null = leave unchanged.</summary>
    public string? Notes { get; set; }
}

public class ChangeBikeLifecycleCommandValidator : AbstractValidator<ChangeBikeLifecycleCommand>
{
    public ChangeBikeLifecycleCommandValidator()
    {
        RuleFor(x => x.Lifecycle).IsInEnum();

        When(x => x.Lifecycle == BikeLifecycle.Sold, () =>
        {
            RuleFor(x => x.SaleDate).NotNull();
            RuleFor(x => x.SalePrice).NotNull().GreaterThanOrEqualTo(0);
        });
    }
}

public class ChangeBikeLifecycleCommandHandler : IRequestHandler<ChangeBikeLifecycleCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public ChangeBikeLifecycleCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(ChangeBikeLifecycleCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IBikeRepository>();
        var bike = await repo.GetBikeAsync(request.Id);

        if (bike is null)
        {
            return Result.Fail(new NotFoundError($"Bike {request.Id} not found."));
        }

        if (bike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
        }

        bike.Lifecycle = request.Lifecycle;

        if (request.Notes is not null)
        {
            bike.Notes = request.Notes;
        }

        if (request.Lifecycle == BikeLifecycle.Sold)
        {
            bike.SaleDate = BikeDateHelper.EnsureUtc(request.SaleDate);
            bike.SalePrice = request.SalePrice;
        }
        else
        {
            // Leaving Sold clears the sale details.
            bike.SaleDate = null;
            bike.SalePrice = null;
        }

        repo.UpdateBike(bike);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}
