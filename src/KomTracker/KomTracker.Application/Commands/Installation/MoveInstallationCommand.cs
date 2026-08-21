using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>Moves an active Tracked installation: closes the current window and opens a new one (atomic).</summary>
public class MoveInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }
    public int NewBikeId { get; set; }
    public InstallationPosition? NewPosition { get; set; }
    public DateTime MoveDate { get; set; }
}

public class MoveInstallationCommandValidator : AbstractValidator<MoveInstallationCommand>
{
    public MoveInstallationCommandValidator()
    {
        RuleFor(x => x.NewBikeId).GreaterThan(0);
        RuleFor(x => x.NewPosition).IsInEnum().When(x => x.NewPosition.HasValue);
    }
}

public class MoveInstallationCommandHandler : IRequestHandler<MoveInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public MoveInstallationCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(MoveInstallationCommand request, CancellationToken cancellationToken)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();

        var current = await installationRepo.GetAsync(request.InstallationId);
        if (current is null)
        {
            return Result.Fail(new NotFoundError($"Installation {request.InstallationId} not found."));
        }

        if (current.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Installation does not belong to the current user."));
        }

        if (!current.IsCurrent)
        {
            return Result.Fail(new ConflictError("Only a currently-installed component can be moved."));
        }

        var newBike = await bikeRepo.GetBikeAsync(request.NewBikeId);
        if (newBike is null)
        {
            return Result.Fail(new NotFoundError($"Bike {request.NewBikeId} not found."));
        }

        if (newBike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
        }

        var moveDate = InstallationDateHelper.EnsureUtc(request.MoveDate);

        // Close the current window.
        current.DateTo = moveDate;
        installationRepo.Update(current);

        // Open a new Tracked window on the new bike/position.
        installationRepo.Add(new InstallationEntity
        {
            UserId = request.UserId,
            ComponentId = current.ComponentId,
            BikeId = request.NewBikeId,
            Type = ComponentInstallationType.Tracked,
            Position = request.NewPosition,
            DateFrom = moveDate,
            DateTo = null
        });

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}
